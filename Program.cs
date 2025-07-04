using System;
using System.Collections.Generic;
using System.Linq;

// Single Responsibility Principle (SRP)
// Each class has one reason to change: Character handles character data, Inventory handles item management
public interface ICharacter
{
    string Name { get; }
    void UseAbility();
}

public class Warrior : ICharacter
{
    public string Name { get; }
    public Warrior(string name) => Name = name;
    public void UseAbility() => Console.WriteLine($"{Name} swings a mighty sword!");
}

public class Mage : ICharacter
{
    public string Name { get; }
    public Mage(string name) => Name = name;
    public void UseAbility() => Console.WriteLine($"{Name} casts a fireball spell!");
}

// Open/Closed Principle (OCP)
// Inventory is open for extension (new item types) but closed for modification
public interface IItem
{
    string ItemName { get; }
    void Use(ICharacter character);
}

public class Sword : IItem
{
    public string ItemName => "Sword";
    public void Use(ICharacter character) => Console.WriteLine($"{character.Name} slashes with {ItemName}!");
}

public class Spellbook : IItem
{
    public string ItemName => "Spellbook";
    public void Use(ICharacter character) => Console.WriteLine($"{character.Name} casts a spell from {ItemName}!");
}

// Generic Inventory class using constraints for type safety
public class Inventory<TItem> where TItem : IItem // only IItem types can be used, if not, it will throw an error
{
    private readonly List<TItem> items = new List<TItem>();

    public void AddItem(TItem item)
    {
        items.Add(item);
        Console.WriteLine($"Added {item.ItemName} to inventory.");
    }

    public void UseAllItems(ICharacter character)
    {
        foreach (var item in items)
        {
            item.Use(character);
        }
    }

    public IEnumerable<TItem> GetAll() => items.AsReadOnly();
}

// Liskov Substitution Principle (LSP)
// SpecialItem can substitute IItem without breaking behavior
public class SpecialItem<T> : IItem where T : IItem
{
    private readonly T baseItem;
    public string ItemName => $"Enhanced {baseItem.ItemName}";
    public SpecialItem(T baseItem) => this.baseItem = baseItem;
    public void Use(ICharacter character)
    {
        Console.WriteLine($"{character.Name} uses {ItemName} with extra power!");
        baseItem.Use(character);
    }
}

// Interface Segregation Principle (ISP)
// IEquipable is a specific interface for equipable items, avoiding forcing unrelated methods
public interface IEquipable
{
    void Equip(ICharacter character);
}

public class Armor : IItem, IEquipable
{
    public string ItemName => "Armor";
    public void Use(ICharacter character) => Console.WriteLine($"{character.Name} wears {ItemName} for protection.");
    public void Equip(ICharacter character) => Console.WriteLine($"{character.Name} equips {ItemName}.");
}

// Dependency Inversion Principle (DIP)
// CharacterManager depends on abstractions (ICharacter, IInventoryService) not concrete classes
public interface IInventoryService<T> where T : IItem
{
    void AddItem(T item);
    void UseAllItems(ICharacter character);
}

public class InventoryService<T> : IInventoryService<T> where T : IItem
{
    private readonly Inventory<T> inventory;
    public InventoryService(Inventory<T> inventory) => this.inventory = inventory;
    public void AddItem(T item) => inventory.AddItem(item);
    public void UseAllItems(ICharacter character) => inventory.UseAllItems(character);
}

public class CharacterManager
{
    private readonly ICharacter character;
    private readonly IInventoryService<IItem> inventoryService;

    public CharacterManager(ICharacter character, IInventoryService<IItem> inventoryService)
    {
        this.character = character;
        this.inventoryService = inventoryService;
    }

    public void PerformActions() // This violates SRP, but is acceptable for demonstration purposes (Single Responsibility Principle)
    {
        character.UseAbility();
        inventoryService.AddItem(new Sword());
        inventoryService.AddItem(new Spellbook());
        inventoryService.UseAllItems(character);
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Create a warrior and inventory
        var warrior = new Warrior("Aragorn");
        var warriorInventory = new Inventory<IItem>();
        var warriorService = new InventoryService<IItem>(warriorInventory);
        var warriorManager = new CharacterManager(warrior, warriorService);

        // Demonstrate warrior actions
        Console.WriteLine("Warrior Actions:");
        warriorManager.PerformActions();

        // Create a mage with special items
        var mage = new Mage("Gandalf");
        var mageInventory = new Inventory<IItem>();
        var mageService = new InventoryService<IItem>(mageInventory);
        mageService.AddItem(new SpecialItem<Sword>(new Sword()));
        mageService.AddItem(new Armor());
        var mageManager = new CharacterManager(mage, mageService);

        // Demonstrate mage actions
        Console.WriteLine("\nMage Actions:");
        mageManager.PerformActions();

        // Equip armor for mage
        if (mageInventory.GetAll().OfType<IEquipable>().FirstOrDefault() is IEquipable equipable)
        {
            equipable.Equip(mage);
        }
    }
}