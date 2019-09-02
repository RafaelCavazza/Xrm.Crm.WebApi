# Xrm.Crm.WebApi

[![Version](https://img.shields.io/nuget/vpre/Xrm.Crm.WebApi.svg)](https://www.nuget.org/packages/Xrm.Crm.WebApi)
[![Downloads](https://img.shields.io/nuget/dt/Xrm.Crm.WebApi.svg)](https://www.nuget.org/packages/Xrm.Crm.WebApi)

Xrm.Crm.WebApi is a library intended to simplify the use of the Microsoft Dynamics Crm REST API.

It was created out of necessity since the official SDK does not support dotnet core.

# Getting started

Currently the library only supports the online version with the Server-to-server authentication.

The on-premisse authentication and the user authentication will be added in the future.

You can read more about the Server-to-server authentication and how to create a Dynamics 365 application user [here](<https://docs.microsoft.com/en-us/previous-versions/dynamicscrm-2016/developers-guide/mt790170(v=crm.8)>).

Initializing the connection:

```c#
    Guid clientId = new Guid("{00000000-0000-0000-0000-000000000000}");
    string clientSecret = "";
    string crmBaseUrl = "https://myOrg.crm.dynamics.com";
    Guid tenantId = new Guid("{00000000-0000-0000-0000-000000000000}");

    ServerToServerAuthentication authentication = new ServerToServerAuthentication(clientId,clientSecret, crmBaseUrl, tenantId);
    WebApi webApi = new WebApi(authentication);
```

# Basic CRUD

All the methos have a Async option.

## Create

### Basic Create

```c#
    Entity contact = new Entity("contact");
    contact["firstname"] = "Jose";
    contact["gendercode"] = 1;
    contact["isbackofficecustomer"] = false;
    contact["overriddencreatedon"] = DateTime.Now.ToUniversalTime();
    Guid contactId = webApi.Create(contact);
```

### Lookup Properties

The library will add the necessary information if you use the 'EntityReference' object to define the relationship value

```c#
    Entity contact = new Entity("contact");
    contact["firstname"] = "Jose";
    contact["accountid"] = new EntityReference("account", new Guid("{00000000-0000-0000-0000-000000000000}"));
    Guid contactId = webApi.Create(contact);
```

The 'EntityReference' type also works with alternate keys

```c#
    Entity contact = new Entity("contact");
    contact["firstname"] = "Jose";
    contact["accountid"] = new EntityReference("account", "my_key_name", "my_value");
    Guid contactId = webApi.Create(contact);
```

You can also explicitly add the relationship

```c#
    Entity contact = new Entity("contact");
    contact["firstname"] = "Jose";
    contact["accountid@odata.bind"] = "/accounts(00000000-0000-0000-0000-000000000000)";
    Guid contactId = webApi.Create(contact);
```

## Retrieve

Currently only 'FetchXml' has been tested and extensively validated when retrieving multiple records.
Other options will be added/improved in the future.

### Using FetchXml
```c#
    string fetchXml = @"
    <fetch count='100'>
        <entity name='contact'>
            <all-attributes />
        </entity>
    </fetch> ";

    RetrieveMultipleResponse retrieveMultipleResponse = webApi.RetrieveMultiple(fetchXml);
    List<Entity> entities = retrieveMultipleResponse.Entities;

    foreach(Entity entity in entities)
    {
        string firstname = entity.GetAttributeValue<string>("firstname");
        DateTime firstname = entity.GetAttributeValue<DateTime>("createdon");
        EntityReference accountid = entity.GetAttributeValue<EntityReference>("accountid");
    }
```

### Single record

All attributes
```c#
    Entity entity = webApi.Retrieve("contact", new Guid("00000000-0000-0000-0000-000000000000"));
```
Only 'firstname' and 'createdon' are selected in the API
```c#
    Entity entity = webApi.Retrieve("contact", new Guid("00000000-0000-0000-0000-000000000000"), "firstname", "createdon");
```

Support for retrieving a single record using an alternate key will be added.

## Update

The update method follows the same logic from the create method. The only difference is: the 'Entity' object needs to have a Id or a alternate key

```c#
    Entity contact = new Entity("contact", new Guid("00000000-0000-0000-0000-000000000000"));
    contact["firstname"] = "Jose";
    webApi.Update(contact);
```

```c#
    Entity contact = new Entity("contact", "my_key_name", "my_key_value");
    contact["firstname"] = "Jose";
    webApi.Update(contact);
```
## Upsert

The upsert method is identical to the Update. You can control its behavior with the 'InsertOptions' parameter. The default option will update the record if it already exists or create it it doesn't 

```c#
    Entity contact = new Entity("contact", new Guid("00000000-0000-0000-0000-000000000000"));
    contact["firstname"] = "Jose";
    webApi.Upsert(contact);
```

## Delete

To delete a entity you only need to provide the logical name, id or alternate key

```c#
    Entity contact = new Entity("contact", new Guid("00000000-0000-0000-0000-000000000000"));
    webApi.Delete(contact);
```

```c#
    Entity contact = new Entity("contact", "my_key_name", "my_key_value");
    webApi.Delete(contact);
```


### Todos

-   Improve the Docs
-   Improve the Todos 😊

## License

MIT
