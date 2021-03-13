using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsSimulator
{
    class Program
    {
        //the dictionary stores all positions occupied by ships, and the ship which is occupying them
        public static Dictionary<Vector2, Ship> dict = new Dictionary<Vector2, Ship>();
        public static Random rand = new Random();

        //the grid is used to print the results, and keeps track of the status of each position
        static Grid grid;

        static int shipsSunk = 0;
        static int turnNumber = 1;

        static void Main(string[] args)
        {
            //spawn the ships and add there occupying positions to the dictionary

            Battleship battleship = new Battleship();
            Destroyer destroyerFirst = new Destroyer();
            Destroyer destroyerSecond = new Destroyer();
           

            //create and print the grid

            grid = new Grid();
            grid.PrintGrid();



            //do the game loop

            bool completed = false;

            do
            {
                //take and process input

                Console.WriteLine("Turn " + turnNumber + ":");
                string input = Console.ReadLine();

                bool validInput = CheckInputValid(input);
                
                if (!validInput)
                {
                    Console.WriteLine("Invalid input. Please enter again. (A-J)+(0-9)");
                    continue;
                }

                Vector2 inputVector = InputToVector(input);


                //execute move

                DealWithInput(inputVector);
                grid.PrintGrid();


                //check if game won

                if (shipsSunk == 3)
                {
                    Console.WriteLine("Congratulations! Game completed in " + turnNumber + " turns");
                    completed = true;
                    Console.ReadLine();
                }

                turnNumber++;
            }
            while (!completed);
           
        }


        static bool CheckInputValid (string s)
        {
            //check the input follows the required format

            if (s.Length != 2)
            {
                return false;
            }

            byte[] values = GetValueOfChar(s);
            if (values[0] < 65 || values[0] > 74)
            {
                return false;
            }
            if (values[1] < 48 || values[1] > 57)
            {
                return false;
            }

            return true;
        }

        static Vector2 InputToVector (string s)
        {
            byte[] values = GetValueOfChar(s);

            return new Vector2(values[0] - 65, values[1] - 48);
        }

        static byte[] GetValueOfChar (string s)
        {
            byte[] asciByte = Encoding.ASCII.GetBytes(s);

            return asciByte;
        }

        public static void AddShipCoordsToDictionary (Ship ship)
        {
            //tell the dictionary that these positions are occupied by this ship

            int length = ship.length;

            if (ship.horizontal)
            {
                for (int i = 0; i < length; i++)
                {
                    dict.Add(new Vector2 (ship.startPos.x + i, ship.startPos.y), ship);
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    dict.Add(new Vector2(ship.startPos.x, ship.startPos.y + i), ship);
                }
            }
        }

        public static Ship CheckWhatIsAtCoords (Vector2 pos)
        {
            Ship ship = null;

            if (dict.ContainsKey(pos))
            {
                dict.TryGetValue(pos, out ship);
            }

            return ship;
        }

        static void DealWithInput (Vector2 input)
        {
            Ship ship = CheckWhatIsAtCoords(input);
            string update;
            string status = grid.CheckWhatIsAtCoords(input);

            //check what is at the position, do the required action, and update the grid

            if (ship is Battleship)
            {
                update = "[X]";
                
                if (status.Equals(update))
                {
                    Console.WriteLine("Already hit these coords");
                    return;
                }
                else
                {
                    if (ship.TakeDamage())
                    {
                        Console.WriteLine("Ship sunk!");
                        shipsSunk++;
                    }
                    else
                    {
                        Console.WriteLine("Hit!");
                    }
                }
            }
            else if (ship is Destroyer)
            {
                update = "[X]";

                if (status.Equals(update))
                {
                    Console.WriteLine("Already hit these coords");
                    return;
                }
                else
                {
                    
                    if (ship.TakeDamage())
                    {
                        Console.WriteLine("Ship sunk!");
                        shipsSunk++;
                    }
                    else
                    {
                        Console.WriteLine("Hit!");
                    }
                }
            }
            else
            {
                Console.WriteLine("Miss");
                update = "[-]";

                if (status.Equals(update))
                {
                    Console.WriteLine("Already tried these coords");
                    return;
                }
            }

            if (!update.Equals(""))
            {
                grid.UpdateGridPosition(input, update);
            }
        }
    }


    class Grid
    {
        string[,] grid = new string[10, 10];

        public Grid ()
        {
            CreateGrid();
        }

        void CreateGrid()
        {
            //i is x, j is y
            for (int j = 0; j < 10; j++)
            {
                for (int i = 0; i < 10; i++)
                {

                    //Code left in for testing. Enables checking that the ships are spawning correctly on creation

                    /*Vector2 currentPos = new Vector2(i, j);
                    Ship ship = Program.CheckWhatIsAtCoords(currentPos);
                    
                    if (ship is Battleship || ship is Destroyer)
                    { 
                        if (ship is Battleship)
                        {
                            grid[i, j] = "[b]";
                        }
                        else
                        {
                            grid[i, j] = "[d]";
                        }

                        continue;
                    }*/


                    grid[i, j] = "[ ]";
                }
            }
        }

        public void PrintGrid()
        {
            Console.WriteLine();
            Console.WriteLine("    A  B  C  D  E  F  G  H  I  J");
            Console.WriteLine();

            //i is x, j is y
            for (int j = 0; j < 10; j++)
            {
                Console.Write(" " + j + " ");

                for (int i = 0; i < 10; i++)
                {
                    Console.Write(grid[i, j]);
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void UpdateGridPosition(Vector2 pos, string s)
        {
            grid[pos.x, pos.y] = s;
        }

        public string CheckWhatIsAtCoords(Vector2 pos)
        {
            return grid[pos.x, pos.y];
        }

    }


    class Ship
    {
        public bool horizontal;
        public Vector2 startPos;
        public int length;
        public int hits = 0;
        public bool sunk = false;

        public bool TakeDamage ()
        {
            hits++;

            if (hits == length)
            {
                sunk = true;
            }

            return sunk;
        }

        protected void Create ()
        {
            //determine whether it will be horizontal or vertical

            horizontal = Program.rand.Next(0, 2) % 2 == 0;


            //find a random start position, and then go through and check that each position the ship would occupy does not clash with another ship (by checking the dictionary of ship positions)

            bool validPosFound;
            do
            {
                validPosFound = true;

                if (horizontal)
                {
                    startPos = new Vector2(Program.rand.Next(0, 10 - (length - 1)), Program.rand.Next(0, 10));
                }
                else
                {
                    startPos = new Vector2(Program.rand.Next(0, 10), Program.rand.Next(0, 10 - (length - 1)));
                }


                for (int i = 0; i < length; i++)
                {
                    Vector2 currentPos;
                    if (horizontal)
                    {
                        currentPos = new Vector2(startPos.x + i, startPos.y);
                    }
                    else
                    {
                        currentPos = new Vector2(startPos.x, startPos.y + i);
                    }

                    Ship otherShip = Program.CheckWhatIsAtCoords(currentPos);
                    if (otherShip is Battleship || otherShip is Destroyer)
                    {
                        validPosFound = false;
                        break;
                    }
                }
            }
            while (!validPosFound);

            //add these positions to the dictionary
            Program.AddShipCoordsToDictionary(this);

        }
    }

    class Battleship : Ship
    {
        public Battleship ()
        {
            length = 5;
            Create();
        }
    }

    class Destroyer : Ship
    {
        public Destroyer()
        {
            length = 4;
            Create();
        }
    }

    struct Vector2
    {
        public int x;
        public int y;

        public Vector2 (int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    };


}


