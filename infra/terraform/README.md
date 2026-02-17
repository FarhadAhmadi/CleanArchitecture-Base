# Terraform Infrastructure

## Initialize
```bash
cd infra/terraform
terraform init
```

## Plan
```bash
terraform plan -var-file=environments/dev.tfvars
```

## Apply
```bash
terraform apply -var-file=environments/dev.tfvars
```

Use `staging.tfvars` and `prod.tfvars` for other environments.
