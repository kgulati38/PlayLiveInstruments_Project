public class Instrument
{
    public string Name { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; }

    public Instrument(string name, string type, decimal price)
    {
        Name = name;
        Type = type;
        Price = price;
    }

    public override string ToString()
    {
        return $"{Name} ({Type}) - ${Price}";
    }
}