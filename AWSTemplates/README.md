# AWS Templates
This project provides a set of AWS CloudFormation SAM templates that can be used to create stacks that create and deploy all the resources our system requires.

The templates are organized to support the incremental deployment of multiple tenancies in a CI/CD pipeline and to support  deployment to a dev environment.

This set of templates describes a system where we have these tenancy types:
- Consumer
- Store
- System


## LazyMagic MDD
LazyMagic MDD generates SAM (Serverless Application Model) templates from the LazyMagic.yaml directives file and template snippets from the AWSTemplates/Snippets folder. 

The stacks and scripts generated into ```AWSTemplates/Generated``` include:
- Deploy-Tenancy-ConsumerTenancy-Stack.g.ps1
- Deploy-Tenancy-StoreTenancy-Stack.g.ps1
- Deploy-Tenancy-SystemTenancy-Stack.g.ps1
- sam.Service.g.yaml 
- sam.StoreTenancy.g.yaml
- sam.ConsumerTenancy.g.yaml

## LazyMagic Stardard Templates
In addition to generated snippets, the ```AWSTemplates\Templates``` folder contains these standard templates:
- sam.artifactsbucket.yaml
- sam.cfpolicies.yaml
- sam.tenant.assets.yaml
- sam.assets.yaml
- sam.webapp.yaml


## Deployment Scripts
The following scripts are provided, in the ```AWSTemplates``` folder, to deploy the system:
- Deploy-Artifacts-Stack.ps1
- Deploy-Assets-Stack.ps1
- Deploy-CFPolicies-Stack.ps1
- Deploy-Service-Stack.ps1
- Deploy-WebApp-Stack.ps1


The following PowerShell scripts are generated into the ```AWSTemplates\Generated``` folder:
- Generated/Deploy-Tenancy-StoreTenancy-Stack.g.ps1  // deploy once for each store. 
- Generated/Deploy-Tenancy-SystemTenancy-Stack.g.ps1 
- Generated/Deploy-Tenancy-ConsumerTenancy-Stack.g.ps1 


## Setting Expectations
These templates describe a non-trivial real-world implementation that can be used as a starting point for your own system.  They are also meant as a learning tool for new practioners.

We have made some assumptions about the system that may not be true for your system. You may need to modify the templates here to address your specific requirements. We have broken up the system into multiple templates using parameters and outputs to make dependencies clear. 

The policies provided in the templates may be too permissive for some use cases. You should review and modify the policies to be more restrictive where necessary. We do follow AWS best practices for security and cost management, but there are too many variations in requirements to cover all possible cases.

## Prerequisites
- A domian name registered with Route53 and the HostedZoneId for the domain name.
- An SSL certificate for the domain name issued by AWS Certificate Manager (ACM) and the ARN for the certificate.

Once you have these, update the serviceconfig.yaml file in the folder above the solution folder. This file is not included in the repository. Here ais the template for the serviceconfig.yaml file:
```
# serviceconfig.yaml file
# This is the config file holding the parameters for the deployment. Each deployment should have its own config file with 
# unique SystemGuid, RootDomain, HostedZoneId, and AcmCertificateArn.
# AWSTemplates/config.yaml is a template. You should copy it to a new file located in the directory above the solution folder 
# and modify the parameters to match your deployment.

SystemGuid: "yourguid" # You must provide your own guid here. This guid is used to uniquely identify the system. Note lower case.
RootDomain: "yourdomain" # You must provide your own root domain here. 
HostedZoneId: "awssupplied" # You must provide your own hosted zone id here.
AcmCertificateArn: "awssuppliedcertificationarn" # You must provide your own certificate arn here.

# You can modify the following parameters to customize the deployment.
SystemName: "lzm" # This is the name of the cloudformation stack that will be created to deploy the system.
S3Prefix: "artifacts" # This is the folder in the s3 bucket where the artifacts are stored. 
Profile: "lzm-dev" # This is the name of the profile in the aws credentials file that will be used to deploy the system.
Environment: 'dev'
```

### SystemName, GUID, and TenantKeys
S3 bucket names are global in scope. They must be unique across all of AWS. To ensure uniqueness, we use a GUID in bucket names. By default, this GUID is the SystemGuid provided in the serviceconfig.yaml file.

S3 bucket names are limited to 63 characters and a GUID is 32 characters long, leaving 31 characters for other bucket description. We use these remaining 31 characters in the following way:
- \{systemName}-\{bucketType}-\{optionalkey-}\{guid}

Since the "\{systemName}-\{bucketType}-\{optionalkey-}" can't exceed 31 characters, our conventions are:
- SystemName: 3 characters max
- BucketType: artifacts, assets-\{tenatkey}, cdnlog-\{tenantkey}, webapp-\{appname}
- TenantKey: 18 characters max
- AppName: 18 characters max

Here are the bucket names for the sample system, using the system name "lzm":
- lzm-artifacts-\{guid}
- lzm-assets-system-\{guid}
- lzm-assets-uptown-\{guid}
- lzm-assets-downtown-\{guid}
- lzm-assets-consumer-\{guid}
- lzm-cdnlog-system-\{guid}
- lzm-cdnlog-uptown-\{guid}
- lzm-cdnlog-downtown-\{guid}
- lzm-cdnlog-consumer-\{guid}
- lzm-webapp-adminapp-\{guid}
- lzm-webapp-storeapp-\{guid}
- lzm-webapp-consumerapp-\{guid}

Note that tenant keys may not be more than 18 characters long. The tenant keys are used in the bucket names and the DynamoDB table names.

The system name, tenant keys, and GUId must be lower case.

#### Using longer System Name and or longer Tenant Keys
If you need a longer system name or tenant key, you can use a shorter GUID. Often, taking the middle portion of the GUID will be unqiue enough. For instance:
- GUID: 500c154a-fb41-496a-bd90-27f541ff523b
- Short GUID: fb41-496a-bd90



## Deployment
To deploy the system, follow these steps:
1. Create and update the serviceconfig.yaml file in the folder above the solution folder.
2. cd into the AWSTemplates folder.

Service - API Gateways, Lambdas, Cognito, and Identity Pools
1. ```.\Deploy-Artifacts-Stack.ps1``` -- creates the S3 bucket for the lambda artifacts.
2. ```.\Deploy-Service-Stack.ps1``` -- creates the APIs, Lambdas, Cognito User Pools, and Identity Pools.

 
WebApps - S3 buckets
1. ```.\Deploy-WebApp-Stack.ps1 -AppName adminapp```
2. ```.\Deploy-WebApp-Stack.ps1 -AppName storeapp```
3. ```.\Deploy-WebApp-Stack.ps1 -AppName consumerapp```

4. Deploy webapps to the buckets. Use deploywebapp.ps1 in the WASMApp project folder.

Assets - S3 buckets and DynamoDB table
1. ```.\Deploy-Assets-Stack.ps1 -TenantKey system```
2. ```.\Deploy-Assets-Stack.ps1 -TenantKey uptown```
3. ```.\Deploy-Assets-Stack.ps1 -TenantKey downtown```
4. ```.\Deploy-Assets-Stack.ps1 -TenantKey consumer```
5. Deploy the assets to the asset buckets. See the MagicPetsTenancies solution

Tenancies - CloudFront Policies, CloudFront distributions with Route53 records
1. ```.\Deploy-CFPolicies-Stack.ps1```
2. ```cd to the Generated folder```
3. ```.\Deploy-Tenant-SystemTenantcy-Stack.ps1 -TenantKey system -SubDomain admin```
4. ```.\Deploy-Tenant-StoreTenancy-Stack.ps1 -TenantKey uptown -SubDomain uptown```
5. ```.\Deploy-Tenant-StoreTenantcy-Stack.ps1 -TenantKey downtown -SubDomain downtown```
6. ```.\Deploy-Tenant-ConsumerTenantcy-Stack.ps1 -TenantKey consumer -SubDomain app```

### Adding New WebApps 

1. Use the ```.\Deploy-WebApp-Stack.ps1``` script to create a new webapp. The script creates an S3 bucket and deploys the webapp to the bucket. 
2. Deploy the webapp to the new webapp bucket. Use deploywebapp.ps1 in the WASMApp project folder.
3. Use the ```Deploy-Tenant-{tenancykey}-Stack.ps1``` script to redeploy any tenancy that uses the new webapp. It doesn't hurt to redeploy all tenancies as AWS will simply update only the resources that have changed.

### Adding New Tenancies
1. Use the ```Deploy-Assets-Stack.ps1``` script to create the new tenancy assets. 
2. Deploy the assets to the asset buckets for the new tenancy. See the MagicPetsTenancies solution.
3. Use the ```Deploy-Tenant-{tenancykey}-Stack.ps1``` script to create a new tenancy. The script creates a CloudFront distribution and Route53 record for the tenancy. It also creates a new DynamoDB table for the tenancy.

Note: You can pass a different GUID, than the System GUID, to the Deploy-Assets-Stack.ps1 script. This is rarely needed, but it is possible for some other AWS account to stomp on your bucket names.

### Deleting the Stacks
You must delete the stacks in the reverse order of creation.

To avoid data loss, the sam.assets.yaml template, used in the Deploy-Assets-Stack.ps1 script, specifies that the S3 buckets and DynamoDB table are not deleted when the stack is deleted.

Note: After you delete the assets stack, you can't deploy it again without deleting the S3 buckets and DynamoDB table.After deleting the stack, you must delete the S3 buckets and DynamoDB table manually. 

Note: One easy way to delete the S3 buckets and DynamoDB table is to use the Visual Studio AWS Explorer.
For a tenancy, the S3 buckets are named:
- \{systemname}-assets-\{tenantkey}-\{guid}
 
The database table is named:
- \{tenantkey}

## Name Scopes
There are some AWS resource naming scope rules that are respected by these generated templates. These rules are:
- S3 bucket names must be unique across all of AWS. We use a GUID in the bucket names to ensure uniqueness.
- OAC names are unique per account. We use a GUID in the OAC names to ensure uniqueness.
- CloudFront function names are unique per account. We use a GUID in the function names to ensure uniqueness.
- DynamoDB table names are unqie per account. Each table name must be unique within a specific AWS region for your account.




## CORS and CSP

Cross-Origin Resource Sharing (CORS) and Content Security Policy (CSP) are important security features that should be enabled in your system.

CORS is a security feature that restricts what resources can be accessed by a web page from another domain. It is important to restrict access to your resources to only the domains that should have access. This is done by setting the Access-Control-Allow-Origin header in the response to the request. The Access-Control-Allow-Origin header should be set to the domain that is allowed to access the resource. If the request is from a domain that is not allowed, the browser will block the request.

Content Security Policy (CSP) is a security feature that restricts what resources can be loaded by a web page. It is important to restrict the resources that can be loaded by your web page to only the resources that are necessary. This is done by setting the Content-Security-Policy header in the response to the request. The Content-Security-Policy header should be set to only allow the resources that are necessary. If the request is for a resource that is not allowed, the browser will block the request.

### CORS
The dev environment needs to allow multiple origins. The webapp is loaded from a different server than that hosting the API. The API may be hosted locally or through CloudFront. In general, we allow all origins in the dev environment.

The prod environment should only allow the domain of the webapp. However, the webapp may be assocaited with a subdomain so the CORS policy must allow the subdomain as well. We currently allow any subdomain of the root domain in our policy.

### CSP

We load two external scripts in our webapp:
    <script src="_framework/blazor.webassembly.js"></script>

Our CSP policy must allow these scripts to be loaded. 
Content-Security-Policy: script-src 'self' https://uptown.lazymagicdev.click/_framework/blazor.webassembly.js


