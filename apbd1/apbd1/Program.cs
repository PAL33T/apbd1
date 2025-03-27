using System;
using System.Collections.Generic;

public interface IHazardNotifier
{
    void NotifyHazard(string containerId);
}

public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public abstract class Container
{
    private static int counter = 1;
    public string Id { get; private set; }
    public double MaxCapacity { get; set; }
    public double CurrentLoad { get; set; }
    public double EmptyWeight { get; set; }

    protected Container(string type, double maxCapacity, double emptyWeight)
    {
        Id = $"KON-{type}-{counter++}";
        MaxCapacity = maxCapacity;
        EmptyWeight = emptyWeight;
        CurrentLoad = 0;
    }
    public abstract void EmptyContainer();
    public abstract void LoadContainer(double load);
}

public class LiquidContainer : Container, IHazardNotifier
{
    public bool IsDangerous { get; private set; }
    
    public LiquidContainer(double maxCapacity, double emptyWeight, bool isDangerous) 
        : base("L", maxCapacity, emptyWeight)
    {
        IsDangerous = isDangerous;
    }

    public void NotifyHazard(string containerId)
    {
        Console.WriteLine($"Uwaga, niebezpieczeństwo w kontenerze: {containerId}");
    }

    public override void LoadContainer(double load)
    {
        if ((CurrentLoad + load) > MaxCapacity * (IsDangerous ? 0.5 : 0.9))
        {
            throw new OverfillException("Przeładowanie");
        }
        CurrentLoad += load;
    }

    public override void EmptyContainer()
    {
        CurrentLoad = 0;
    }
}

public class GasContainer : Container, IHazardNotifier
{
    public double Pressure { get; set; }

    public GasContainer(double maxCapacity, double emptyWeight, double pressure) 
        : base("G", maxCapacity, emptyWeight)
    {
        Pressure = pressure;
    }

    public override void EmptyContainer()
    {
        CurrentLoad *= 0.05;
    }

    public override void LoadContainer(double load)
    {
        if (CurrentLoad + load > MaxCapacity)
        {
            throw new OverfillException("Kontener przeciążony");
        }
        CurrentLoad += load;
    }

    public void NotifyHazard(string containerId)
    {
        Console.WriteLine($"Uwaga, niebezpieczeństwo w kontenerze: {containerId}");
    }
}

public class CooledContainer : Container
{
    public double Temperature { get; private set; }
    
    public CooledContainer(double maxCapacity, double emptyWeight, double temperature) 
        : base("C", maxCapacity, emptyWeight)
    {
        Temperature = temperature;
    }

    public override void LoadContainer(double load)
    {
        if (CurrentLoad + load > MaxCapacity)
        {
            throw new OverfillException("Przeładowanie");
        }
        CurrentLoad += load;
    }

    public override void EmptyContainer()
    {
        CurrentLoad = 0;
    }
}

public class ContainerShip
{
    public string Name { get; private set; }
    public double MaxSpeed { get; private set; }
    public int MaxContainers { get; private set; }
    public double MaxWeight { get; private set; }
    private List<Container> containers = new List<Container>();

    public ContainerShip(string name, double maxSpeed, int maxContainers, double maxWeight)
    {
        Name = name;
        MaxSpeed = maxSpeed;
        MaxContainers = maxContainers;
        MaxWeight = maxWeight;
    }

    public void AddContainer(Container container)
    {
        if (containers.Count >= MaxContainers)
            throw new Exception("Przekroczono maksymalną liczbę kontenerów");
        if (GetTotalWeight() + container.EmptyWeight + container.CurrentLoad > MaxWeight)
            throw new Exception("Przekroczono maksymalną wagę statku");
        
        containers.Add(container);
    }

    public void RemoveContainer(string id)
    {
        containers.RemoveAll(c => c.Id == id);
    }

    public double GetTotalWeight()
    {
        double weight = 0;
        foreach (var container in containers)
        {
            weight += container.EmptyWeight + container.CurrentLoad;
        }
        return weight;
    }

    public void PrintShipInfo()
    {
        Console.WriteLine($"Statek: {Name}, Prędkość: {MaxSpeed} węzłów, Kontenery: {containers.Count}/{MaxContainers}, Ładowność: {GetTotalWeight()}/{MaxWeight} kg");
        foreach (var container in containers)
        {
            Console.WriteLine($"  - {container.Id}, Załadowano: {container.CurrentLoad}/{container.MaxCapacity} kg");
        }
    }
}

class Program
{
    static void Main()
    {
        try
        {
            ContainerShip ship = new ContainerShip("Ocean Carrier", 20, 5, 50000);

            LiquidContainer liquidContainer = new LiquidContainer(100, 10, true);
            ship.AddContainer(liquidContainer);
            liquidContainer.LoadContainer(40);
            Console.WriteLine("Załadowano LiquidContainer");

            GasContainer gasContainer = new GasContainer(50, 5, 10);
            ship.AddContainer(gasContainer);
            gasContainer.LoadContainer(30);
            Console.WriteLine("Załadowano GasContainer");

            CooledContainer cooledContainer = new CooledContainer(80, 15, -5);
            ship.AddContainer(cooledContainer);
            cooledContainer.LoadContainer(60);
            Console.WriteLine("Załadowano CooledContainer");

            ship.PrintShipInfo();

            ship.RemoveContainer(liquidContainer.Id);
            Console.WriteLine("Usunięto LiquidContainer");
            ship.PrintShipInfo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }
    }
}
