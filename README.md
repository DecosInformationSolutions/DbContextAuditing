# Decos.Data.Auditing

[![Build status](https://dev.azure.com/decos/Decos%20Core/_apis/build/status/Decos.Data.Auditing)](https://dev.azure.com/decos/Decos%20Core/_build/latest?definitionId=230)
[![NuGet version](https://badge.fury.io/nu/Decos.Data.Auditing.EntityFrameworkCore.svg)](https://badge.fury.io/nu/Decos.Data.Auditing.EntityFrameworkCore)

Records changes made in an Entity Framework Core database context.

Most commonly used types:

- Decos.Data.Auditing.EntityFrameworkCore.AuditedContext

## Quick start

Using the *Decos.Data.Auditing.EntityFrameworkCore* namespace, make sure your database context derives from the *AuditedContext* class, and add the following line to the *OnModelCreating* method to prepare the context for storing change sets and changes:

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseAuditing();
    }
    
In addition, a custom *Decos.Data.Auditing.IIdentity* implementation needs to be created. This part will likely be moved to a separate package in the future. For now, implement the *Id* and *Name* properties to return the appropriate values. 

At the moment, we support the ASP.NET Core Dependency Injection abstractions. Configure your context and then call the *AddDbContextAuditing* method to configure auditing. The first type parameter specifies the type of context to use; the second parameter specifies the *IIdentity* implementation mentioned earlier.

    services.AddDbContext<TestDbContext>(options =>
    {
        options.UseSqlite("Data Source=Test.db");
    });
    services.AddDbContextAuditing<TestDbContext, TestIdentity>()

**If you're using migrations, don't forget to add a new migration:**

    PM> Add-Migration AddDbContextAuditing
