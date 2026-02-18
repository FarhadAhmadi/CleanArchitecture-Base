# Modular Monolith Structure

## هدف
یکپارچه‌سازی ساختار پروژه به‌صورت ماژول‌محور، بدون شکستن قراردادهای فعلی API و بدون تغییر رفتار runtime.

## ساختار جدید

### Domain
- `src/Domain/Modules/Users`
- `src/Domain/Modules/Authorization`
- `src/Domain/Modules/Auditing`
- `src/Domain/Modules/Files`
- `src/Domain/Modules/Logging`
- `src/Domain/Modules/Notifications`
- `src/Domain/Modules/Todos`

### Application
- `src/Application/Modules/Users`
- `src/Application/Modules/Authorization`
- `src/Application/Modules/Todos`

### Infrastructure
- `src/Infrastructure/Modules/<ModuleName>/Infrastructure/*`
- `src/Infrastructure/Database/Configurations/<ModuleName>/*`

### Web API
- `src/Web.Api/Endpoints/Modules/<ModuleName>/*`

## قواعد
1. کد اختصاصی هر ماژول فقط داخل پوشه همان ماژول قرار بگیرد.
2. کدهای cross-cutting در ریشه‌های مشترک بمانند:
   - `src/Infrastructure/Database`
   - `src/Infrastructure/Caching`
   - `src/Infrastructure/Messaging`
3. endpoint جدید فقط در مسیر:
   - `src/Web.Api/Endpoints/Modules/<ModuleName>`
4. feature جدید application فقط در مسیر:
   - `src/Application/Modules/<ModuleName>/<Feature>`

## تغییرات انجام‌شده
1. تمامی EF configurations در یک محل مرکزی تجمیع شد:
   - `src/Infrastructure/Database/Configurations`
2. پوشه‌های ماژولی در Domain/Application/Web.Api/Infrastructure به ساختار `Modules` منتقل شد.
3. اسکریپت scaffolding پروژه با ساختار جدید هماهنگ شد:
   - `scripts/feature/new-feature.ps1`
4. mapping endpointها در runtime به صورت module-aware مرتب شد:
   - `src/Web.Api/Extensions/EndpointExtensions.cs`
