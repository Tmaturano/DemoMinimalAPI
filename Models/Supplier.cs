namespace DemoMinimalAPI.Models
{
    public class Supplier
    {
        public Guid Id { get; private set; }
        public string? Name { get; set; }
        public string? Document { get; set; }
        public bool Active { get; set; }

        public Supplier()
        {
            Id = Guid.NewGuid();
        }

        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}
