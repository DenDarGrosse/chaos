using System;
using System.Threading;

namespace ServerConsole
{
    class SeverConsole
    {
        const int PORT = 52527;

        static void Main(string[] args)
        {
            Console.WriteLine("run server? Y/N");
            if (Console.ReadLine().Trim().ToUpper() == "Y")
            {
                string host = System.Net.Dns.GetHostName();
                System.Net.IPAddress ip = System.Net.Dns.GetHostByName(host).AddressList[0];
                Console.WriteLine("Ip adress: " + ip.ToString());

                Server server = new Server(PORT, Console.Out);
                Thread commands = new Thread(Commands);
                commands.Start();
                server.Work();

                void Commands()
                {
                    while (true)
                    {
                        string command = Console.ReadLine();
                        string[] com = command.Split(' ');
                        switch (com[0])
                        {
                            case "players":
                                Console.WriteLine("Players: ");
                                foreach (ClientInfo c in server.clients)
                                {
                                    Console.WriteLine(c.clientName);
                                }
                                break;
                            case "rooms":
                                Console.WriteLine("Rooms:");
                                foreach(Room r in server.rooms)
                                {
                                    Console.WriteLine(r.name);
                                }
                                break;
                            case "resourses":
                                foreach(ClientInfo c in server.clients)
                                {
                                    if (c.clientName == com[1])
                                    {
                                        switch (com[2])
                                        {
                                            case "wood":
                                                c.room.res[c.myTurnNumber, 1] += int.Parse(com[3]);
                                                break;
                                            case "rock":
                                                c.room.res[c.myTurnNumber, 2] += int.Parse(com[3]);
                                                break;
                                            case "iron":
                                                c.room.res[c.myTurnNumber, 0] += int.Parse(com[3]);
                                                break;
                                            case "food":
                                                c.room.res[c.myTurnNumber, 3] += int.Parse(com[3]);
                                                break;
                                            case "free":
                                                c.room.res[c.myTurnNumber, 4] += int.Parse(com[3]);
                                                break;
                                            default:
                                                Console.WriteLine($"There isn't type: {com[3]}");
                                                break;
                                        }
                                        return;
                                    }
                                }
                                Console.WriteLine($"there isn't player: {com[1]}");
                                break;
                            case "help":
                                Console.WriteLine("players" + Environment.NewLine +
                                    "rooms" + Environment.NewLine +
                                    "resourses player type(wood,rock,iron,food,free) count" + Environment.NewLine +
                                    "");
                                break;
                            default:
                                Console.WriteLine($"There isn't command: {com[0]}");
                                break;
                        }
                    }
                }
            }
        }
    }
}
