public class TestDemo
{
    // Property to hold an Id
    public int Id { get; set; }

    // Property to hold a name
    public string Name { get; set; }

    // Property to hold a description
    public string Description { get; set; }

    // Property to hold the creation date
    public DateTime CreatedDate { get; set; }

    // Constructor
    public TestDemo()
    {
        CreatedDate = DateTime.Now;
    }
}
