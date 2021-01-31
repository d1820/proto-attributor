namespace ProtoAttributor.Services
{
    public class AttributeService : IAttributeService
    {
        private Microsoft.VisualStudio.OLE.Interop.IServiceProvider _serviceProvider;

        public AttributeService(Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp)
        {
            _serviceProvider = sp;
        }

        public void Hello()
        {
        }

        public string Goodbye()
        {
            return "goodbye";
        }
    }
}
