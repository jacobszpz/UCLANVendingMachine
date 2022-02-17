using System;
using System.Collections.Generic;

namespace UCLANVendingMachine
{
    class VendingMachine
    {
        // Vending Machine Product Catalogue
        protected static List<Product> products = new List<Product>();
        // Stores User Credit
        protected static double Credit { get; set; } = 0;
        // Stores Selected Items
        protected static List<int> selection = new List<int>();

        // Used to simplify all text output
        protected const int indent = 4;
        protected const int separatorLen = indent * 10;
        protected const char separator = '.', dash = '-';
        protected static string indentStr = new string(' ', indent);
        protected static string dashStr = new string(dash, separatorLen);
        protected static string separatorStr = new string(separator, separatorLen);

        // Stores a single product
        // With a name and a cost
        // These are not modifiable
        public struct Product
        {
            public Product (string name, double cost) 
            {
                this.Name = name;
                this.Cost = cost;
            }

            public string Name { get; }
            public double Cost { get; }
            public override string ToString() => $"{Name}: £{Cost}";
        }

        public static void Main(string[] args)
        {
            // Used to know when to quit main loop, submenu selection loop
            bool running = true, submenuSelected = false;

            // Add initial products to inventory, with their respective prices
            AddProduct("Chocolate Bar", 0.80);
            AddProduct("Soda Can", 0.70);
            AddProduct("Soda Bottle", 1.25);
            AddProduct("Crisps", 0.50);
            AddProduct("Cookies", 1.10);

            // Repeats main menu until user quits program
            while (running)
            {
                // Clear console and print main menu
                Console.Clear();
                DisplayMain();

                // Wrap submenu selection in loop to avoid reprinting if user input is incorrect
                while (!submenuSelected)
                {
                    submenuSelected = true;

                    switch (AskDigit())
                    {
                        // Handle valid menu choices
                        case ConsoleKey.D1:
                            AddCreditsDisplay();
                            break;
    
                        case ConsoleKey.D2:
                            ProductSelection();
                            break;

                        // Change main loop condition if user chooses to exit
                        case ConsoleKey.D0:
                        case ConsoleKey.Escape:
                            running = false;
                            break;
    
                        default:
                            // Do not escape submenu selection mode
                            submenuSelected = false;
                            Console.WriteLine("Option not recognized, try again");
                            break;
                    }
                }

                // Reset submenu selection variable for next display of main menu
                submenuSelected = false;
            }

            // Display goodbye message
            Console.WriteLine("Thanks for choosing UCLAN Vending Machines LTD");
            Console.WriteLine("Have a nice day!");
        }

        // Prompts the user for a single digit (key, really)
        protected static ConsoleKey AskDigit()
        {
            Console.Write("Please enter a number: ");
            ConsoleKey submenu = Console.ReadKey().Key;
            Console.WriteLine();
            Console.WriteLine();
            return submenu;
        }

        // Prompts the user for a line containing a number
        // Used instead of previous function for product selection
        // Taking into account a vending machine might have more than 9 products
        protected static string AskNumber()
        {
            Console.Write("Please enter a number, then press enter: ");
            String submenu = Console.ReadLine();
            Console.WriteLine();
            return submenu;
        }

        // Create a product object and add it to the inventory
        // only if it isn't there already
        public static bool AddProduct(string name, double cost)
        {
            Product newProduct = new Product(name, cost);

            if (!products.Contains(newProduct)) 
            {
                products.Add(newProduct);
                return true;
            }

            return false;
        }

        // Increases user credits by specified amount
        protected static bool AddCredits(double increase) 
        {
            Credit += increase;
            return false;
        }

        // Adds up the cost of every product in user selection
        protected static double CalculateTotal()
        {
            double total = 0;

            foreach (int product in selection)
            {
                total += products[product].Cost;
            }

            return total;
        }

        // Discount selection cost from user credits
        protected static bool MakePurchase()
        {
            double remaining = Credit - CalculateTotal();

            if (remaining > 0)
            {
                Credit = remaining;
                return true;
            }


            return false;

        }

        // User menu for adding credits
        protected static void AddCreditsDisplay()
        {
            bool validInput = false;
            double addCredits = 0;

            while (!validInput)
            {
                Console.Write("Please enter how many credits you would like to add to your balance: ");

                // Input is only valid if correctly parsed and non-negative
                validInput = double.TryParse(Console.ReadLine(), out addCredits);
                validInput = validInput && (addCredits >= 0);

                if (!validInput)
                {
                    Console.WriteLine("Invalid amount of credits, try again (just input 0 to cancel).");
                }

                Console.WriteLine();
            }

            AddCredits(addCredits);
            Console.WriteLine(dashStr);
            Console.WriteLine();
            Console.WriteLine($"Your new Balance{indentStr}= {Credit:0.00} credits");
            Console.WriteLine();
            Console.ReadKey();
        }

        // Main menu prompt
        protected static void DisplayMain() 
        {
            string titleBorder;
            string company = "UCLAN Vending Machines LTD";

            titleBorder = new String(dash, company.Length);

            List<string> menu = new List<string>();
            menu.Add(titleBorder);
            menu.Add(company);
            menu.Add(titleBorder);
            menu.Add("MAIN MENU");
            menu.Add($"{indentStr}1. Add Credits (current credits = {Credit:0.00})");
            menu.Add($"{indentStr}2. Select product(s)");
            menu.Add($"{indentStr}0. Exit");

            foreach (string line in menu)
            {
                Console.WriteLine(line);
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine();
        }
        
        // Product selection menu
        // Handles user interaction logic
        protected static void ProductSelection()
        {
            bool productSelected = false, selectionDone = false, enoughCredits;

            // Display inventory just once
            ProductSelectionDisplay();

            // Product selection loop
            while (!productSelected || !selectionDone)
            {
                if (int.TryParse(AskNumber(), out int product))
                {
                    // Ensure selected item number is within inventory count
                    if (product >= 0 && product <= products.Count)
                    {
                        // User chose to cancel selection
                        if (product == 0)
                        {
                            selection.Clear();
                            return;
                        }

                        // Adjust to 0-based index
                        --product;
                        productSelected = true;

                        // Add to selection
                        selection.Add(product);
                        NewSelectionDisplay(products[product].Name);
                        ConsoleKey response = Console.ReadKey().Key;
                        Console.WriteLine();
                        Console.WriteLine();

                        // If not done selecting, repeat prompt
                        if (response != ConsoleKey.Y)
                        {
                            selectionDone = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Option not recognized, try again");
                    }
                }
                else
                {
                    Console.WriteLine("Option not recognized, try again");
                }
            }

            // Payment loop
            do
            {
                // Calculate total
                TotalDisplay();
                enoughCredits = (CalculateTotal() <= Credit);

                // Should user hold enough credits to make purchase, proceed
                if (enoughCredits)
                {
                    MakePurchase();
                    SuccessPurchaseDisplay();
                }
                // Otherwise, offer to add credits
                else
                {
                    InsuficientCreditsDisplay();
                    ConsoleKey moreCreditsAnswer = Console.ReadKey().Key;
                    Console.WriteLine();

                    if (moreCreditsAnswer != ConsoleKey.Y)
                    {
                        selection.Clear();
                        return;
                    }

                    AddCreditsDisplay();
                    Console.WriteLine("Would you like to continue processing your order?");
                    Console.WriteLine();
                    Console.Write($"{indentStr}(Press 'Y' to process order, any other key to cancel) : ");

                    ConsoleKey continueOrder = Console.ReadKey().Key;
                    Console.WriteLine();

                    if (continueOrder != ConsoleKey.Y)
                    {
                        selection.Clear();
                        return;
                    }

                }
            } while (!enoughCredits);

            // Clear out basket
            selection.Clear();
        }

        // Prompt in case of a successful purchase
        protected static void SuccessPurchaseDisplay()
        {
            List<string> display = new List<string>();
            display.Add($"Your new balance{indentStr}= {Credit:0.00} credits");
            display.Add(dashStr);
            display.Add("Thank you for your custom!");

            foreach (string line in display)
            {
                Console.WriteLine(line);
                Console.WriteLine();
            }

            Console.ReadKey();
        }

        // Prompt used whenever a purchase failed due to insufficiency
        protected static void InsuficientCreditsDisplay()
        {
            double needed = CalculateTotal() - Credit;
            List<string> display = new List<string>();
            display.Add($"You have insufficient credits available. You require \"{needed:0.00}\" credits.");
            display.Add($"{indentStr}Would you like to add more credits?");

            foreach (string line in display)
            {
                Console.WriteLine(line);
                Console.WriteLine();
            }
            
            Console.Write($"{indentStr}(Press 'Y' to add credits, any other key to cancel) : ");
        }

        // Display bill when purchasing one or more products
        protected static void TotalDisplay() 
        {   
            List<string> total = new List<string>();
            total.Add(separatorStr);
            total.Add($"Available Balance    = {Credit:0.00} credits");
            total.Add($"Grand Total          = {CalculateTotal():0.00} credits");
            total.Add(dashStr);

            foreach (string line in total)
            {
                Console.WriteLine(line);
                Console.WriteLine();
            }
        }

        // Displayed when a product is added to basket
        protected static void NewSelectionDisplay(string name)
        {
            Console.WriteLine($"{indentStr}You have added \"{name}\" to your selection.");
            Console.WriteLine($"{indentStr}Your current sub total = {CalculateTotal():0.00} credits.");
            Console.WriteLine();
            Console.WriteLine($"{indentStr}Would you like to add another product?");
            Console.Write($"{indentStr}(Press 'Y' to add another, any other key to skip) : ");
        }

        // Print out inventory menu
        protected static void ProductSelectionDisplay()
        {
            Console.WriteLine($"PRODUCT SELECTION    [Current Balance = {Credit:0.00} credits]");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Please choose from the following options:");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            int i = 1;
            int maxL = 0;

            // Get max line length for pretty-printing
            foreach (Product product in products)
            {
                maxL = Math.Max(maxL, product.Name.Length);
            }

            // Print every product along its item number
            foreach (Product product in products) 
            {
                string relSpacing = new string(' ', maxL - product.Name.Length);
                Console.WriteLine($"{indentStr}{i}. {product.Name}{indentStr}{relSpacing}[{product.Cost:0.00} credits]");
                Console.WriteLine();
                i++;
            }

            Console.WriteLine($"{indentStr}0. Return to Main Menu");
            Console.WriteLine();
            Console.WriteLine(new string(separator, separatorLen));
            Console.WriteLine();
        }
    }
}
