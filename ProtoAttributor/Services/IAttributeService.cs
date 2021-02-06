namespace ProtoAttributor.Services
{
    public interface IAttributeService
    {
        string AddAttributes(string fileContents);

        string RemoveAttributes(string fileContents);

        string ReorderAttributes(string fileContents, int startingIndex = 1);

    }
}
