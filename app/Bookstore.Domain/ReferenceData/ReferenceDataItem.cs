namespace Bookstore.Domain.ReferenceData
{
    public class ReferenceDataItem : Entity
    {
        // An empty constructor is required by EF Core
        private ReferenceDataItem() { }

        public ReferenceDataItem(ReferenceDataType referenceDataType, string text)
        {
            DataType = referenceDataType;
            Text = text;
        }

        // What an item is, is settled when it is created. If it could be changed, a genre that
        // books were already filed under could quietly become a publisher.
        public ReferenceDataType DataType { get; private set; }

        // The empty constructor above (required by EF Core) leaves this unset; the real
        // constructor and Rename are the only places that assign it.
        public string Text { get; private set; } = null!;

        // The only thing about an item that can change: what it is called.
        public void Rename(string text)
        {
            Text = text;
        }
    }
}
