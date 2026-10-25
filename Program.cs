using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "General";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
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
            Console.WriteLine("3. Search product (by ID or name)");
            Console.WriteLine("4. Update stock");
            Console.WriteLine("5. Delete product");
            Console.WriteLine("6. Total stock value");
            Console.WriteLine("7. Low stock alert");
            Console.WriteLine("8. View by category");
            Console.WriteLine("9. Export to CSV");
            Console.WriteLine("0. Exit");
            Console.Write("Choose: ");

            switch (Console.ReadLine()?.Trim())
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
                case "8":
                    ViewByCategory();
                    break;
                case "9":
                    ExportCsv();
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

    // ---------- Input helpers ----------

    static int ReadInt(string message, int min = 0)
    {
        while (true)
        {
            Console.Write(message);
            if (int.TryParse(Console.ReadLine(), out int n) && n >= min)
                return n;

            Console.WriteLine($"Enter a whole number >= {min}.");
        }
    }

    static decimal ReadPrice(string message)
    {
        while (true)
        {
            Console.Write(message);
            if (decimal.TryParse(Console.ReadLine(), out decimal d) && d >= 0)
                return d;

            Console.WriteLine("Enter a valid price (0 or more).");
        }
    }

    static string ReadText(string message)
    {
        while (true)
        {
            Console.Write(message);
            string? text = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(text))
                return text;

            Console.WriteLine("This field cannot be empty.");
        }
    }

    static string ReadCategory()
    {
        Console.Write("Category (press Enter for General): ");
        string? text = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(text) ? "General" : text;
    }

    static Product? FindProduct(int id)
    {
        return products.FirstOrDefault(p => p.Id == id);
    }

    static void PrintProduct(Product p)
    {
        Console.WriteLine($"ID: {p.Id} | {p.Name} | {p.Category} | Qty: {p.Quantity} | Price: KSh {p.Price:N2}");
    }

    // ---------- Menu actions ----------

    static void AddProduct()
    {
        int id = ReadInt("Product ID: ", 1);

        if (FindProduct(id) != null)
        {
            Console.WriteLine("A product with that ID already exists.");
            return;
        }

        string name = ReadText("Name: ");
        string category = ReadCategory();
        int quantity = ReadInt("Quantity: ");
        decimal price = ReadPrice("Price (KSh): ");

        products.Add(new Product
        {
            Id = id,
            Name = name,
            Category = category,
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

        foreach (var p in products.OrderBy(x => x.Id))
        {
            PrintProduct(p);
        }
    }

    static void SearchProduct()
    {
        Console.WriteLine("Search by: 1. ID   2. Name");
        Console.Write("Choose: ");
        string? choice = Console.ReadLine()?.Trim();

        if (choice == "1")
        {
            int id = ReadInt("Enter product ID: ", 1);
            var found = FindProduct(id);

            if (found == null)
                Console.WriteLine("Product not found.");
            else
                PrintProduct(found);
        }
        else if (choice == "2")
        {
            string term = ReadText("Enter part of the name: ");
            var matches = products
                .Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Id)
                .ToList();

            if (matches.Count == 0)
            {
                Console.WriteLine("No products match that name.");
                return;
            }

            foreach (var m in matches)
            {
                PrintProduct(m);
            }
        }
        else
        {
            Console.WriteLine("Invalid choice.");
        }
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

        p.Quantity = ReadInt("New quantity: ");
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
        Console.WriteLine("Product deleted.");
    }

    static void StockValue()
    {
        decimal total = products.Sum(x => x.Quantity * x.Price);
        Console.WriteLine($"Total stock value: KSh {total:N2}");
    }

    static void LowStockAlert()
    {
        var low = products.Where(x => x.Quantity < lowStockLimit).ToList();

        if (low.Count == 0)
        {
            Console.WriteLine("All stock levels OK.");
            return;
        }

        Console.WriteLine($"Products with less than {lowStockLimit} in stock:");
        foreach (var p in low)
        {
            Console.WriteLine($"[!] {p.Name} (ID {p.Id}) - only {p.Quantity} left");
        }
    }

    static void ViewByCategory()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("No products available.");
            return;
        }

        var groups = products
            .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key);

        Console.WriteLine("Categories:");
        foreach (var g in groups)
        {
            Console.WriteLine($"- {g.Key} ({g.Count()} products)");
        }

        string name = ReadText("Category to view: ");
        var items = products
            .Where(x => x.Category.Equals(name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Id)
            .ToList();

        if (items.Count == 0)
        {
            Console.WriteLine("No products in that category.");
            return;
        }

        foreach (var item in items)
        {
            PrintProduct(item);
        }

        decimal value = items.Sum(x => x.Quantity * x.Price);
        Console.WriteLine($"Category stock value: KSh {value:N2}");
    }

    static void ExportCsv()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("No products to export.");
            return;
        }

        string path = $"inventory_export_{DateTime.Now:yyyyMMdd_HHmm}.csv";
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Category,Quantity,Price,StockValue");

        foreach (var p in products.OrderBy(x => x.Id))
        {
            sb.AppendLine(
                $"{p.Id}," +
                $"{Csv(p.Name)}," +
                $"{Csv(p.Category)}," +
                $"{p.Quantity}," +
                $"{p.Price.ToString("F2", inv)}," +
                $"{(p.Quantity * p.Price).ToString("F2", inv)}");
        }

        try
        {
            File.WriteAllText(path, sb.ToString());
            Console.WriteLine($"Exported to: {Path.GetFullPath(path)}");
        }
        catch (IOException)
        {
            Console.WriteLine("Could not write the CSV file.");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("No permission to write the CSV file here.");
        }
    }

    // Wraps a value in quotes if it contains a comma or quote, so Excel reads it correctly.
    static string Csv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    // ---------- Storage ----------

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