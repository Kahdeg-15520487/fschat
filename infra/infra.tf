provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

// user which I want to give permissions to be able to access the keyvault.
data "azuread_user" "user"{
    user_principal_name = "jonhenry13241_gmail.com#EXT#@jonhenry13241gmail.onmicrosoft.com"
}

variable "admin_login" {}
variable "admin_password" {}

resource "azurerm_resource_group" "resource_group" {
  name     = "xmas-pentest"
  location = "eastasia"
}

resource "azurerm_service_plan" "app_service_plan" {
  name                = "xmas-app-service-plan"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  sku_name            = "F1"
  os_type             = "Windows"
}

resource "azurerm_windows_web_app" "be" {
  name                = "be-fschat"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  service_plan_id     = azurerm_service_plan.app_service_plan.id

  site_config {
    always_on = false
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_windows_web_app" "fe" {
  name                = "fe-fschat"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  service_plan_id     = azurerm_service_plan.app_service_plan.id

  site_config {
    always_on = false
  }
}

resource "azurerm_key_vault" "key_vault" {
  name                            = "xmas-key-vault"
  location                        = azurerm_resource_group.resource_group.location
  resource_group_name             = azurerm_resource_group.resource_group.name
  enabled_for_disk_encryption     = false
  enabled_for_deployment          = false
  enabled_for_template_deployment = false

  sku_name  = "standard"
  tenant_id = data.azurerm_client_config.current.tenant_id
}

resource "azurerm_key_vault_access_policy" "example" {
  key_vault_id = azurerm_key_vault.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azuread_user.user.object_id

  secret_permissions = [
    "Set",
    "Get",
    "List"
  ]
}

resource "azurerm_key_vault_access_policy" "be_web_app_access_policy" {
    key_vault_id = azurerm_key_vault.key_vault.id
    tenant_id    = data.azurerm_client_config.current.tenant_id
    object_id    = azurerm_windows_web_app.be.identity.0.principal_id

    secret_permissions = [
        "Get",
        "List"
    ]
}

resource "azurerm_mssql_server" "db_server" {
  name                         = "chat-sqlserver"
  resource_group_name          = azurerm_resource_group.resource_group.name
  location                     = azurerm_resource_group.resource_group.location
  version                      = "12.0"
  administrator_login          = var.admin_login
  administrator_login_password = var.admin_password
}

resource "azurerm_sql_database" "db" {
    name                = "chat-database"
    resource_group_name = azurerm_resource_group.resource_group.name
    server_name         = azurerm_mssql_server.db_server.name
    location            = azurerm_resource_group.resource_group.location
    requested_service_objective_name = "Basic"
}