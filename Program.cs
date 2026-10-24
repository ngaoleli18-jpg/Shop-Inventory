using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Product
{
    public int Id {get; set; }
    public string Name {get; set; } = "";
     public int Quantity {get; set; }
      public decimal Price {get; set; }
}

class Program
{
    static List<Product> products = new();
    static string file = "inventory.json";
    static int lowStockLimit = 5;

    static void Main()
    {
        LoadData();

        while (true)
        {
            Console.WriteLine("\n=== SHOP INVENTORY SYSTEM ===");
            Console.WriteLine("1. Add product");
             Console.WriteLine("2. View products");
              Console.WriteLine("3. Search product");
               Console.WriteLine("4. Update stock");
                Console.WriteLine("5. Delete product");
                 Console.WriteLine("6. Total stock value");
                 Console.WriteLine("7. Low stock alert");
                  Console.WriteLine("0. Exit");
                   Console.Write("Choose: ");

                   switch (Console.ReadLine())
            {
                case "1":
                 AddProduct();
                break;
                   case "2":
                    ViewProducts(); 
                break;
                   case "3":
                    SearchProduct(); 
                break;
                   case "4":
                    UpdateStock(); 
                break;
                   case "5":
                    DeleteProduct(); 
                break;
                   case "6":
                    StockValue(); 
                break;
                case "7":
                LowStockAlert();
                break;
                   case "0": 
                   SaveData();
                   return;
                   default:

                   Console.WriteLine("Invalid choice.");
                break;
            }
        }
    }

    static int ReadInt(string message, int min = 0)
    {
        while (true)
        {
            Console.Write(message);
            if
            (int.TryParse(Console.ReadLine(), out int n) && n >=min)
            return n;

            Console.WriteLine($"Enter a whole number >= {min}.");
        }
    }
    
    static decimal ReadPrice(string message)
    {
        while (true)
        {
            Console.Write(message);
            if
            (decimal.TryParse(Console.ReadLine(), out decimal p) && p >= 0)
            return p;
            Console.WriteLine("Enter a valid non-negative price");
        }
    }

    static Product? FindProduct(int id) => products.FirstOrDefault(p => p.Id == id);

    static void AddProduct()
    {
        int id = ReadInt("Product ID: ", 1);

        if (FindProduct(id) != null)
        {
            Console.WriteLine("That ID already exists.");
            return;
        }

        Console.Write("Product name: ");
        string name = Console.ReadLine()?.Trim() ??"";

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannot be empty.");
            return;
        }

        int quantity = ReadInt("Quantity: ");
        decimal price = ReadPrice("Price (KSh): ");

        products.Add(new Product
        {
            Id = id,
            Name = name,
            Quantity = quantity,
            Price = price
        });


        SaveData();
        Console.WriteLine("Product added successfully.");
    }

    static void ViewProducts()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("No products available.");
            return;
        }

        foreach (var p in products.OrderBy(p=> p.Id))
        {
            Console.WriteLine($"ID: {p.Id} | {p.Name} | Qty: {p.Quantity} | " + $"Price: KSh {p.Price:N2}");
        }
    }

    static void SearchProduct()
    {
        int id = ReadInt("Enter product ID: ", 1);
        var p = FindProduct(id);
        if (p == null)
        Console.WriteLine("Product not found.");
        else
        Console.WriteLine($"{p.Name} | Quantity: {p.Quantity} | " + $"Price: KSh {p.Price:N2}");
    }

    static void UpdateStock()
    {
        int id = ReadInt("Product ID: ", 1);
        var p = FindProduct(id);

        if (p == null)
        {
            Console.WriteLine("Product not found.");
            return;
        }
            p.Quantity = ReadInt("NewQuantity: ");
            p.Price = ReadPrice("New price (KSh): ");

            SaveData();
            Console.WriteLine("Stock updated.");
        }

        static void DeleteProduct()
        {
            int id = ReadInt("Product ID: ", 1);
            var p = FindProduct(id);

            if (p == null)
            {
                Console.WriteLine("Product not found.");
                return;
            }

            products.Remove(p);
            SaveData();
            Console.WriteLine("Product deleted. ");
        }

        static void StockValue()
        {
            decimal total = products.Sum(p => p.Quantity * p.Price);
            Console.WriteLine($"Total stock value: KSh {total:N2}");
        }

        static void LowStockAlert()
    {
        var low = products.Where(p => p.Quantity < lowStockLimit).ToList();
        if (low.Count == 0)
        {
            Console.WriteLine("All stock levels OK");
            return;
        }
        Console.WriteLine($"Products with less than {lowStockLimit} in stock:");
        foreach (var p in low)
        {
            Console.WriteLine($"[!] {p.Name} (ID {p.Id}- only {p.Quantity} left)");
        }
    }

        static void SaveData()
        {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(file, JsonSerializer.Serialize(products, options));
        }
        catch (IOException)
        {
            Console.WriteLine("Could not save data.");
        }
        }

        static void LoadData()
        {
            if (!File.Exists(file))
            return;

        try
        {
            string json = File.ReadAllText(file);
            products = JsonSerializer.Deserialize<List<Product>>(json) ?? new();
        }
        catch (Exception ex) when (ex is IOException || ex is JsonException)
        {
            Console.WriteLine("Could not load data. Starting with an empty list.");
        }
           
        }

    }

