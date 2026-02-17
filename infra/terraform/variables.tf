variable "project_name" {
  description = "Project short name."
  type        = string
  default     = "cleanarch"
}

variable "environment" {
  description = "Deployment environment."
  type        = string
}

variable "location" {
  description = "Azure region."
  type        = string
  default     = "East US"
}

variable "tenant_id" {
  description = "Azure tenant ID for Key Vault."
  type        = string
}

variable "admin_object_id" {
  description = "Azure AD object ID that can administer Key Vault."
  type        = string
}

variable "sql_admin_login" {
  description = "SQL admin login."
  type        = string
}

variable "sql_admin_password" {
  description = "SQL admin password."
  type        = string
  sensitive   = true
}
