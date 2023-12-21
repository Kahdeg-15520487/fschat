
using be.DAL;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace be.Services
{
    public class PdfService : IPdfService
    {
        private ChatDataContext dbContext;
        private IConverter converter;

        public PdfService(ChatDataContext dbContext, IConverter converter)
        {
            this.dbContext = dbContext;
            this.converter = converter;
        }

        public async Task<Stream> GeneratePdfFromChat(Guid groupId)
        {
            var room = await dbContext.Groups.FindAsync(groupId);
            if (room == null)
            {
                return Stream.Null;
            }

            var messages = await dbContext.Messages.Include(m => m.Sender).Where(m => m.GroupID == groupId).ToListAsync();

            var stringbuilder = new StringBuilder();
            stringbuilder.AppendLine("<body>");
            stringbuilder.AppendLine($"<h1>Chat log for {room.GroupID}</h1><br/>");
            foreach (var message in messages)
            {
                stringbuilder.AppendLine($"{message.Sender.UserName}: {message.Content} <br/>");
            }
            stringbuilder.AppendLine("</body>");

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4Plus,
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = stringbuilder.ToString(),
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { FontName= "Segoe UI", FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
            };

            return new MemoryStream(converter.Convert(doc));
        }
    }
}
