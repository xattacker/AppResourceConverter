
namespace Resource.Convert
{
    enum PropertyType : ushort
    {
        RESOURCE = 0,
        EMPTY_LINE,
        COMMENTS,
        OTHERS
    }

    struct PropertyValue
    {
        public PropertyType Type;
        public string Content;
    }
}
