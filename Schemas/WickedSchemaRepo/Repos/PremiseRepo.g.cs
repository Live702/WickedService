
//----------------------
// <auto-generated>
//  Generated by LazyMagic. Create overrides and partial method implementations in a separate file.
//  See the README.g.md file for best practices for extending these generated classes.
// </auto-generated>
//----------------------
namespace WickedSchemaRepo;
public partial interface IPremiseRepo : IDocumentRepo<Premise> {}
public partial class PremiseRepo : DYDBRepository<Premise>, IPremiseRepo
{
    public PremiseRepo(IAmazonDynamoDB client) : base(client) {}
}
