# AWS Templates
This project provides a set of AWS CloudFormation SAM templates that can be used to create stacks that create and deploy all the resources our system requires.

The templates are organized to support the incremental deployment of multiple tenancies in a CI/CD pipeline and to support  deployment to a dev environment.

This set of templates describes a system where we have two tenancy types:
- Store
- Consumer

## LazyMagic MDD
LazyMagic MDD generates SAM (Serverless Application Model) templates from the LazyMagic.yaml directives file and template snippets from the AWSTemplates/Snippets folder. 

The stacks generated into ```AWSTemplates/Generated``` include:
- sam.Service.g.yaml 
- sam.StoreTenancy.g.yaml
- sam.ConsumerTenancy.g.yaml

## LazyMagic Stardard Templates
In addition to generated snippets, the ```AWSTemplates\Templates``` folder contains these standard templates:
- sam.artifactsbucket.yaml
- sam.policies.yaml
- sam.webappbuckets.yaml
- sam.tenant.assets.yaml

## Deployment Scripts
The following PowerShell scripts are provided, in the ```AWSTemplates``` folder, to deploy the system, run them in this order:
- DeployArtifactsStack.ps1
- DeployServiceStack.ps1
- DeployPoliciesStack.ps1
- DeployWebAppStack.ps1
- DeployTenantAssetsStack.ps1  // deploy once for each tenant.
- DeployStoreTenanctStack.ps1  // deploy once for each tenant. 
- DeployConsumerTeantStack.ps1 


## Setting Expectations
These templates describe a non-trivial real-world implementation that can be used as a starting point for your own system.  They are also meant as a learning tool for new practioners.

We have made some assumptions about the system that may not be true for your system. You may need to modify the templates here to address your specific requirements. We have broken up the system into multiple templates using parameters and outputs to make dependencies clear. 

The policies provided in the templates may be too permissive for some use cases. You should review and modify the policies to be more restrictive where necessary. We do follow AWS best practices for security and cost management, but there are too many variations in requirements to cover all possible cases.

## Prerequisites


## Usage 
To deploy the system, follow these steps:
1. Create and update the serviceconfig.yaml file in the folder above the solution folder.
2. cd into the AWSTemplates folder.
3. ```.\DeployArtifactsStack.ps1``` -- creates the S3 bucket for the lambda artifacts.
4. ```.\DeployServiceStack.ps1``` -- creates the APIs, Lambdas, Cognito User Pools, and Identity Pools.
5. ```.\DeployPoliciesStack.ps1``` -- creates the policies used throughout the system.
6. ```.\DeployWebAppStack.ps1``` -- creates the S3 buckets for the web applications.
7. ```.\DeployTenantAssetsStack.ps1 -TenantKey uptown``` -- creates the S3 bucket for the uptown tenant.
8. ```.\DeployStoreTenantStack.ps1 -TenantKey uptown -SubDomain uptown``` -- creates the CloudFront distribution for the uptown tenant.
9. ```.\DeployTenantAssetsStack.ps1 -TenantKey downtown``` -- creates the S3 bucket for the downtown tenant.
10. ```.\DeployStoreTenantStack.ps1 -TenantKey downtown -SubDomain downtown``` -- creates the CloudFront distribution for the downtown tenant.
11. ```.\DeployTenantAssetsStack.ps1 -TenantKey consumer``` -- creates the S3 bucket for the consumer tenant.
12. ```.\DeployConsumerTenantStack.ps1 -TenantKey consumer ``` -- creates the CloudFront distribution for the consumer tenant.

## Notes
Currently, the folowing templates and scripts must be manually updated to reflect changes in the LazyMagic.yaml file:
- sam.webappbuckets.yaml
- DeployWebAppStack.ps1
- DeployStoreTenantStack.ps1
- DeployConsumerTenantStack.ps1

LazyMagic MDD will be updated to generate these files when time permits.


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


