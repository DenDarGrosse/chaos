using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System;

class Server
{
    public TcpListener listener;
    public List<ClientInfo> clients = new List<ClientInfo>();
    public List<ClientInfo> newclients = new List<ClientInfo>();
    public List<Room> rooms = new List<Room>();
    public List<ClientInfo> disconnected = new List<ClientInfo>();
    public static Server server;
    static System.IO.TextWriter Out;

    private int number = 0;

    public Server(int port, System.IO.TextWriter _Out)
    {
        Out = _Out;
        Server.server = this;

        //создаем слушателя

        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        //запускаем
    }

    //для создания потока прослушки
    static void ListnerClients()
    {
        while (true)
        {
            server.newclients.Add(new ClientInfo(server.listener.AcceptTcpClient()));
            //Out.WriteLine("New Client");
        }
    }

    //в сети ли ещё чел
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if(c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }
    
    //рассылка всем пользователям
    private void Broadcast(string data, List<ClientInfo> cl)
    {
        foreach(ClientInfo sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.Client.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch(Exception e)
            {
                Out.WriteLine(e.Message);
            }
        }
    }

    //рассылка определённому пользователю
    private void Broadcast(string data, ClientInfo c)
    {
        List<ClientInfo> sc = new List<ClientInfo> { c };
        Broadcast(data, sc);
    }

    //обработка пришедшей информации
    private void OnIncomingData( string data, ClientInfo c)
    {
        //Out.WriteLine(data);
        string[] aData = data.Split('|');
       // Out.WriteLine(aData[0]);

        switch (aData[0])
        {
            //назначение капитолиев
            case "SetCap":
                for (int i = 1; i < aData.Length; i++)
                {
                    if (i == 1)
                    {
                        Capital cap = new Capital();
                        cap.x = float.Parse(aData[1]);
                        cap.y = float.Parse(aData[2]);
                        cap.owner = c.room.players.ElementAt<ClientInfo>(0);
                        c.room.builds.ElementAt<Builds>(c.room.players.ElementAt<ClientInfo>(0).myTurnNumber).caps.Add(cap);
                        string msgg = "SetCap|";
                        msgg += cap.x + "|";
                        msgg += cap.y;
                        Broadcast(msgg, cap.owner);
                    }
                    if (i == 3)
                    {
                        Capital cap = new Capital();
                        cap.x = float.Parse(aData[3]);
                        cap.y = float.Parse(aData[4]);
                        cap.owner = c.room.players.ElementAt<ClientInfo>(1);
                        c.room.builds.ElementAt<Builds>(c.room.players.ElementAt<ClientInfo>(1).myTurnNumber).caps.Add(cap);
                        string msgg = "SetCap|";
                        msgg += cap.x + "|";
                        msgg += cap.y;
                        Broadcast(msgg, cap.owner);
                    }
                    if (i == 5)
                    {
                        Capital cap = new Capital();
                        cap.x = float.Parse(aData[5]);
                        cap.y = float.Parse(aData[6]);
                        cap.owner = c.room.players.ElementAt<ClientInfo>(2);
                        c.room.builds.ElementAt<Builds>(c.room.players.ElementAt<ClientInfo>(2).myTurnNumber).caps.Add(cap);
                        string msgg = "SetCap|";
                        msgg += cap.x + "|";
                        msgg += cap.y;
                        Broadcast(msgg, cap.owner);
                    }
                    if (i == 7)
                    {
                        Capital cap = new Capital();
                        cap.x = float.Parse(aData[7]);
                        cap.y = float.Parse(aData[8]);
                        cap.owner = c.room.players.ElementAt<ClientInfo>(3);
                        c.room.builds.ElementAt<Builds>(c.room.players.ElementAt<ClientInfo>(3).myTurnNumber).caps.Add(cap);
                        string msgg = "SetCap|";
                        msgg += cap.x + "|";
                        msgg += cap.y;
                        Broadcast(msgg, cap.owner);
                    }
                }
                break;

            //постройка дороги
            case "BuildRoad":
                Broadcast(data, c.room.players);
               // Out.WriteLine(data);
                break;

            //движение
            case "CMOV":
                Out.WriteLine("pereslalPos");
                foreach (Knight k in c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight)
                {
                    if (k.x == float.Parse(aData[2]) || k.z == float.Parse(aData[3]))
                    {
                        k.x = float.Parse(aData[5]);
                        k.z = float.Parse(aData[6]);
                        Broadcast(data, c.room.players);
                        //Out.WriteLine("Knight was moved");
                    }
                    else
                        Out.WriteLine(c.clientName + " is cheater!");
                }
                break;

            //Создание юнита
            case "NU":
                switch (aData[4])
                {
                    case "knight":
                        Out.WriteLine($"Юнит: {aData[5]} {aData[6]} {aData[7]} {aData[8]} {aData[9]} {aData[10]}");
                        if (c.room.res[c.myTurnNumber, 4] < 1)
                            Out.WriteLine($"{c.clientName} читак ебаный");
                        Knight knight = new Knight();
                        knight.owner = c;
                        knight.name = aData[4] + number;
                        number++;
                        knight.x = float.Parse(aData[1]);
                        knight.z = float.Parse(aData[3]);
                        knight.equpmnet[1] = int.Parse(aData[5]);
                        knight.equpmnet[0] = int.Parse(aData[6]);
                        knight.equpmnet[2] = int.Parse(aData[7]);
                        if (aData[8] == "true") knight.equpmnet[3] = 1;
                        else knight.equpmnet[3] = 0;
                        if (aData[9] == "true") knight.equpmnet[4] = 1;
                        else knight.equpmnet[4] = 0;
                        if (aData[10] == "true") knight.equpmnet[5] = 1;
                        else knight.equpmnet[5] = 0;

                        switch (knight.equpmnet[0])
                        {
                            case 1:
                                //knight.stats[0] += 
                                //knight.stats[1] += 
                                //knight.stats[3] +=
                                break;
                            case 2:
                                //attackMan +=
                                //attacHorse +=
                                //deaftMan +=
                                //deaftHorse +=
                                //speed
                                break;
                            case 3:
                                //attacHorse +=
                                //attackMan +=
                                //deaftMan +=
                                //deaftHorse +=
                                //speed
                                break;
                        }
                        switch (knight.equpmnet[1])
                        {
                            case 1:
                                //attackMan +=
                                //attacHorse +=
                                //deaftHorse +=
                                //deaftMan +=
                                //speed
                                break;
                            case 2:
                                //attackMan +=
                                //attacHorse +=
                                //deaftHorse +=
                                //deaftMan +=
                                //idArmor +=   
                                //speed
                                break;
                        }
                        switch (knight.equpmnet[2])
                        {
                            case 1:
                                //attackMan +=
                                //attacHorse +=
                                //deaftHorse +=
                                //deaftMan +=
                                //speed
                                break;
                            case 2:
                                //attackMan +=
                                //attacHorse +=
                                //deaftHorse +=
                                //deaftMan +=
                                //speed
                                break;
                        }
                        if (knight.equpmnet[3] == 1)
                        {
                            //attackMan +=
                            //attacHorse +=
                            //deaftHorse +=
                            //deaftMan +=
                            //speed+=
                        }
                        if (knight.equpmnet[5] == 1)
                        {
                            //attackMan +=
                            //attacHorse +=
                            //deaftHorse +=
                            //deaftMan +=
                            //speed+=
                        }
                        if (knight.equpmnet[4] == 1)
                        {

                        }

                        foreach (MilitaryCamp mc in c.room.builds[c.myTurnNumber].militaryCamp)
                            if (mc.x == float.Parse(aData[1]) && mc.z == float.Parse(aData[3]) + 1)
                                mc.unit = knight;

                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.Add(knight);
                        c.room.res[c.myTurnNumber, 4]--;
                        Broadcast(data + knight.name, c.room.players);
                        break;
                }
                break;

            //Попытка атаковать
            case "TryA":
                //нахождение дефендера
                for (int i = 0; i < c.room.builds.Count; i++)
                {
                    //Out.WriteLine("1");
                    if (aData[6] == "knight")
                    {
                      //  Out.WriteLine("2");
                        for (int j = 0; j < c.room.builds.ElementAt<Builds>(i).knight.Count; j++)
                        {
                          //  Out.WriteLine("3");
                            if (c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).x == float.Parse(aData[3]) && c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).z == float.Parse(aData[4]))
                            {
                              //  Out.WriteLine("4");
                                //нахождение аттакера
                                if (aData[5] == "knight")
                                {
                                  //  Out.WriteLine("5");
                                    for (int d = 0; d < c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.Count; d++)
                                        if (c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).x == float.Parse(aData[1]) && c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).z == float.Parse(aData[2]))
                                        {
                                           // Out.WriteLine("6");
                                            int Pd = (c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).stats[1] * c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).health) - (c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).stats[1] * c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).health);
                                            int Pa = (c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).stats[1] * c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).health);
                                            Out.WriteLine("Потери защищающихся: " + Pd);
                                            Out.WriteLine("Потери атакующих: " + Pa);

                                            //отправка дефендеру о потерях
                                            if (c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).health <= Pd)
                                            {
                                                Broadcast("UnitDie|" + c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).name, c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).owner);
                                                c.room.res[c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).owner.myTurnNumber, 4] += c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).health;
                                                c.room.builds.ElementAt<Builds>(i).knight.RemoveAt(j);
                                            }
                                            else
                                            {
                                                c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).health -= Pd;
                                                Broadcast("UnitDamage|" + c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).name + "|" + c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).health, c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).owner);
                                                c.room.res[c.room.builds.ElementAt<Builds>(i).knight.ElementAt<Knight>(j).owner.myTurnNumber, 4] += Pd;
                                            }

                                            //отправка атакеру о потерях
                                            if (c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).health <= Pa)
                                            {
                                                Broadcast("UnitDie|" + c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).name, c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).owner);
                                                c.room.res[c.myTurnNumber, 4] += c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).health;
                                                c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.RemoveAt(j);
                                            }
                                            else
                                            {
                                                c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).health -= Pa;
                                                Broadcast("UnitDamage|" + c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).name + "|" + c.room.builds.ElementAt<Builds>(c.myTurnNumber).knight.ElementAt<Knight>(d).health, c);
                                                c.room.res[c.myTurnNumber, 4] += Pa;
                                            }
                                        }
                                }
                            }
                        }
                    }
                }
                break;

            //атака здания
            case "AB":
                for (int i = 0; i < 4; i++)
                    foreach (Capital cap in c.room.builds[i].caps)
                    {
                        if (cap.x == float.Parse(aData[1]) && cap.y == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    cap.health -= unit.stats[0];
                                    if (cap.health < 1)
                                    {
                                        Broadcast($"AB|{cap.name}", c.room.players);
                                        c.room.builds[i].caps.Remove(cap);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                for (int i = 0; i < 4; i++)
                    foreach (WindMill wm in c.room.builds[i].windMills)
                    {
                        if (wm.x == float.Parse(aData[1]) && wm.z == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    wm.health -= unit.stats[0];
                                    if (wm.health < 1)
                                    {
                                        Broadcast($"AB|{wm.name}", c.room.players);
                                        c.room.builds[i].windMills.Remove(wm);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                for (int i = 0; i < 4; i++)
                    foreach (Bakery bk in c.room.builds[i].bakery)
                    {
                        if (bk.x == float.Parse(aData[1]) && bk.z == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    bk.health -= unit.stats[0];
                                    if (bk.health < 1)
                                    {
                                        Broadcast($"AB|{bk.name}", c.room.players);
                                        c.room.builds[i].bakery.Remove(bk);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                for (int i = 0; i < 4; i++)
                    foreach (Ambar ar in c.room.builds[i].ambar)
                    {
                        if (ar.x == float.Parse(aData[1]) && ar.z == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    ar.health -= unit.stats[0];
                                    if (ar.health < 1)
                                    {
                                        Broadcast($"AB|{ar.name}", c.room.players);
                                        c.room.builds[i].ambar.Remove(ar);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                for (int i = 0; i < 4; i++)
                    foreach (Rudnik rk in c.room.builds[i].rudnik)
                    {
                        if (rk.x == float.Parse(aData[1]) && rk.z == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    rk.health -= unit.stats[0];
                                    if (rk.health < 1)
                                    {
                                        Broadcast($"AB|{rk.name}", c.room.players);
                                        c.room.builds[i].rudnik.Remove(rk);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                for (int i = 0; i < 4; i++)
                    foreach (House hs in c.room.builds[i].house)
                    {
                        if (hs.x == float.Parse(aData[1]) && hs.z == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    hs.health -= unit.stats[0];
                                    if (hs.health < 1)
                                    {
                                        Broadcast($"AB|{hs.name}", c.room.players);
                                        c.room.builds[i].house.Remove(hs);
                                    }

                                    return;
                                }
                            }
                        }
                    }
                for (int i = 0; i < 4; i++)
                    foreach (MilitaryCamp mc in c.room.builds[i].militaryCamp)
                    {
                        if (mc.x == float.Parse(aData[1]) && mc.z == float.Parse(aData[2]))
                        {
                            foreach (Knight unit in c.room.builds[c.myTurnNumber].knight)
                            {
                                if (unit.x == float.Parse(aData[3]) && unit.z == float.Parse(aData[4]))
                                {
                                    mc.health -= unit.stats[0];
                                    if (mc.health < 1)
                                    {
                                        Broadcast($"AB|{mc.name}", c.room.players);
                                        c.room.builds[i].militaryCamp.Remove(mc);
                                    }
                                    return;
                                }
                            }
                        }
                    }
                break;

            //чат
            case "CHT":
                Out.WriteLine("Chat: " + aData[1]);
                Broadcast(data, c.room.players);
                break;

            //строительство поля
            case "BuildF":
                Broadcast(data, c.room.players);
                foreach (WindMill wm in c.room.builds.ElementAt<Builds>(c.myTurnNumber).windMills)
                {
                    if (wm.x == float.Parse(aData[4]) && wm.z == float.Parse(aData[5]))
                    {
                        Field field = new Field();
                        field.owner = c;
                        field.x = float.Parse(aData[1]);
                        field.z = float.Parse(aData[3]);
                        wm.fields.Add(field);
                    }
                }
                break;

            //строительство
            case "Build":
                Out.WriteLine($"Building: {aData[4]} at {aData[1]} {aData[3]}");
                //Broadcast(data, c.room.players);
                switch (aData[4])
                {
                    case "Rudnik(Clone)":
                        Rudnik rudnik = new Rudnik();
                        rudnik.owner = c;
                        rudnik.x = float.Parse(aData[1]);
                        rudnik.z = float.Parse(aData[3]);
                        rudnik.spezialization = int.Parse(aData[5]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).rudnik.Add(rudnik);
                        rudnik.name = aData[4] + number;
                        data += rudnik.name;
                        number++;
                        break;
                    case "Military Camp(Clone)":
                        if (c.room.res[c.myTurnNumber, 1] < 1 && c.room.res[c.myTurnNumber, 2] < 4)
                        {
                            Out.WriteLine(c.clientName + " is a cheater!");
                        }
                        c.room.res[c.myTurnNumber, 1] -= 1;
                        c.room.res[c.myTurnNumber, 2] -= 4;
                        MilitaryCamp mc = new MilitaryCamp();
                        mc.owner = c;
                        mc.x = float.Parse(aData[1]);
                        mc.z = float.Parse(aData[3]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).militaryCamp.Add(mc);
                        mc.name = aData[4] + number;
                        data += mc.name;
                        number++;
                        break;
                    case "Ambar(Clone)":
                        if (c.room.res[c.myTurnNumber, 1] < 2)
                        {
                            Out.WriteLine(c.clientName + " is a cheater!");
                        }
                        c.room.res[c.myTurnNumber, 1] -= 2;
                        Ambar ambar = new Ambar();
                        ambar.owner = c;
                        ambar.x = float.Parse(aData[1]);
                        ambar.z = float.Parse(aData[3]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).ambar.Add(ambar);
                        ambar.name = aData[4] + number;
                        data += ambar.name;
                        number++;
                        break;
                    case "Bakery(Clone)":
                        if (c.room.res[c.myTurnNumber, 1] < 2 && c.room.res[c.myTurnNumber, 2] < 1)
                        {
                            Out.WriteLine(c.clientName + " is a cheater!");
                        }
                        c.room.res[c.myTurnNumber, 1] -= 2;
                        c.room.res[c.myTurnNumber, 2] -= 1;
                        Bakery bakery = new Bakery();
                        bakery.owner = c;
                        bakery.x = float.Parse(aData[1]);
                        bakery.z = float.Parse(aData[3]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).bakery.Add(bakery);
                        bakery.name = aData[4] + number;
                        data += bakery.name;
                        number++;
                        break;
                    case "WindMill(Clone)":
                        if (c.room.res[c.myTurnNumber, 1] < 2)
                        {
                            Out.WriteLine(c.clientName + " is a cheater!");
                        }
                        c.room.res[c.myTurnNumber, 1] -= 2;
                        WindMill windMill = new WindMill();
                        windMill.owner = c;
                        windMill.x = float.Parse(aData[1]);
                        windMill.z = float.Parse(aData[3]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).windMills.Add(windMill);
                        windMill.name = aData[4] + number;
                        data += windMill.name;
                        number++;
                        break;
                    case "House(Clone)":
                        if (c.room.res[c.myTurnNumber, 1] < 1)
                        {
                            Out.WriteLine(c.clientName + " is a cheater!");
                        }
                        c.room.res[c.myTurnNumber, 1] -= 1;
                        House house = new House();
                        house.owner = c;
                        house.x = float.Parse(aData[1]);
                        house.z = float.Parse(aData[3]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).house.Add(house);
                        house.name = aData[4] + number;
                        data += house.name;
                        number++;
                        break;

                    case "test":
                        MilitaryCamp mcc = new MilitaryCamp();
                        mcc.owner = c;
                        mcc.x = float.Parse(aData[1]);
                        mcc.z = float.Parse(aData[3]);
                        c.room.builds.ElementAt<Builds>(c.myTurnNumber).militaryCamp.Add(mcc);
                        mcc.name = aData[4] + number;
                        data += mcc.name;
                        number++;
                        break;
                }
                foreach (ClientInfo ci in clients)
                    if (ci != c)
                        Broadcast(data, ci);
                data += c.room.res[c.myTurnNumber, 0] + "|";
                data += c.room.res[c.myTurnNumber, 1] + "|";
                data += c.room.res[c.myTurnNumber, 2] + "|";
                data += c.room.res[c.myTurnNumber, 3] + "|";
                data += c.room.res[c.myTurnNumber, 4] + "|";
                Broadcast(data, c);
                break;

            //закончить ход
            case "ENT":
                Out.WriteLine("Zakonchil: " + aData[1]);
                c.room.EndTurn(c, aData);
                Broadcast("EndTurn", c);
                break;

            //новая комната
            case "NR":
               // Out.WriteLine("new room: " + aData[1]);
                Room room = new Room();
                room.id = rooms.Count;
                room.name = aData[1];
                room.mainPlayer = c;
                room.players.Add(c);
                Broadcast("NR|" + room.mainPlayer.clientName, c);
                rooms.Add(room);
                c.room = room;
                Out.WriteLine("Room's name is: " + room.name);
                Out.WriteLine("Room's id is: " + room.id);
                Out.WriteLine("Main Player is: " + room.mainPlayer.clientName);
                break;

            //подключение к комнате
            case "CNNCT":
                for (int i = 0; i < rooms.Count; i++)
                {
                    if (rooms.ElementAt<Room>(i).id == int.Parse(aData[1]))
                    {
                        c.room = rooms.ElementAt<Room>(i);
                        rooms.ElementAt<Room>(i).players.Add(c);
                        string msgg = "CNNCT|";
                        msgg += rooms.ElementAt<Room>(i).mainPlayer.clientName;
                        msgg += rooms.ElementAt<Room>(i).Players();
                       // Out.WriteLine(msgg);
                        Broadcast(msgg, c);
                        string msgForHost = "CNNCTH|";
                        msgForHost += rooms.ElementAt<Room>(i).Players();
                        Out.WriteLine(c + " is connected to " + rooms.ElementAt<Room>(i).name);

                    }
                }
                break;

            //обновление списка комнат
            case "ReLoad":
                string msg = "rooms|";
                foreach (Room r in rooms)
                {
                    msg += r.name + "|";
                }
                //Out.WriteLine(msg);
                Broadcast(msg, c);
                break;

            //установка ника (часть атворизации)
            case "SetName":
                c.clientName = aData[1];
                Out.WriteLine("Новый игрок: " + c.clientName);
                break;

            //ответ на запрос ника(в скором времени уберу)
            case "AskName":
                Out.WriteLine("Your name is: " + c.clientName);
                break;

            //Стал готов к старту
            //делаю два раза цикл, потому что у меня выдал ошибку про кодировку, когда свич под цикл, под иф загнал
            case "Ready":
                switch (aData[1])
                {
                    //Стал готов
                    case "On":
                        c.IsReady = true;
                        Broadcast("Ready|On", c);
                        foreach (ClientInfo client in c.room.players)
                        {
                            if (client == c.room.mainPlayer)
                                continue;
                            else if (client.IsReady != true)
                                return;
                        }
                       // Out.WriteLine("EveryBody are ready");
                        Broadcast("ALLR", c.room.mainPlayer);
                        break;
                    //стал не готов
                    case "Off":
                        c.IsReady = false;
                        Broadcast("Ready|Off", c);
                        break;
                }
                break;
            case "go":
                if (c.room.mainPlayer.clientName == c.clientName)
                    c.room.StartGame(c);
                break;
            case "SetA":
                foreach (Ambar a in c.room.builds.ElementAt<Builds>(c.myTurnNumber).ambar)
                {
                    if (a.x == float.Parse(aData[1]) && a.z == float.Parse(aData[2]))
                    {
                        foreach (WindMill w in c.room.builds.ElementAt<Builds>(c.myTurnNumber).windMills)
                        {
                            if (w.x == float.Parse(aData[3]) && w.z == float.Parse(aData[4]))
                            {
                                w.ambar = a;
                            }
                        }
                    }
                }
                break;
            case "SetB":
                foreach (Bakery b in c.room.builds.ElementAt<Builds>(c.myTurnNumber).bakery)
                {
                    if (b.x == float.Parse(aData[1]) && b.z == float.Parse(aData[2]))
                    {
                        foreach (WindMill w in c.room.builds.ElementAt<Builds>(c.myTurnNumber).windMills)
                        {
                            if (w.x == float.Parse(aData[3]) && w.z == float.Parse(aData[4]))
                            {
                                w.bakery = b;
                            }
                        }
                    }
                }
                break;
            case "SetF":
                foreach (House h in c.room.builds.ElementAt<Builds>(c.myTurnNumber).house)
                {
                    if (h.x == float.Parse(aData[1]) && h.z == float.Parse(aData[2]))
                    {
                        c.room.res[c.myTurnNumber, 3] -= 1;
                        c.room.res[c.myTurnNumber, 4] += 1;
                        h.isFull = true;
                        //c.room.Res(c);
                    }
                }
                break;
            case "AR":
                foreach (MilitaryCamp mc in c.room.builds[c.myTurnNumber].militaryCamp)
                {
                    if (mc.x == float.Parse(aData[1]) && mc.z == float.Parse(aData[2]))
                    {
                        mc.countOfReserv += int.Parse(aData[3]);
                        c.room.res[c.myTurnNumber, 4] -= int.Parse(aData[3]);
                        data += $"|{c.room.res[c.myTurnNumber, 4]}";
                        Broadcast(data, c);
                    }
                }
                break;
            case "SR":
                foreach (MilitaryCamp mc in c.room.builds[c.myTurnNumber].militaryCamp)
                {
                    if (mc.x == float.Parse(aData[1]) && mc.z == float.Parse(aData[2]))
                    {
                        mc.unit.health += int.Parse(aData[3]);
                        mc.countOfReserv -= int.Parse(aData[4]);
                    }
                }
                break;
            case "Enter":
                foreach (MilitaryCamp mc in c.room.builds[c.myTurnNumber].militaryCamp)
                {
                    if (mc.x == float.Parse(aData[1]) && mc.z == float.Parse(aData[2]))
                    {
                        mc.isInside = true;
                    }
                }
                break;
            case "Exit":
                foreach (MilitaryCamp mc in c.room.builds[c.myTurnNumber].militaryCamp)
                {
                    if (mc.x == float.Parse(aData[1]) && mc.z == float.Parse(aData[2]))
                    {
                        mc.isInside = false;
                    }
                }
                break;
        }
    }

    public void Work()
    {
        Thread clientListner = new Thread(ListnerClients);
        clientListner.Start();
        while (true)
        {
            //проверка в сети ли
            foreach(ClientInfo c in clients)
            {
                if (!IsConnected(c.Client))
                {
                    c.Client.Close();
                    disconnected.Add(c);
                    Out.WriteLine(c.clientName + " отключился");
                    continue;
                }
                else
                {
                    //приём инфы
                    NetworkStream s = c.Client.GetStream();
                    if (s.DataAvailable)
                    {
                        StreamReader reader = new StreamReader(s, true);
                        string data = reader.ReadLine();

                        if(data != null)
                        {
                            OnIncomingData( data, c);
                        }
                    }
                }

            }

            //удаление вышедших пользователей
            for(int i = 0; i < disconnected.Count-1; i++)
            {
                //tell out player somebody has disconected

                clients.Remove(disconnected[i]);
                disconnected.RemoveAt(i);
            }
            //добавление пришедших пользователей
            clients.AddRange(newclients);
            if (newclients.Count > 0)
            {
                //логика для получения ника при авторизации
                string msg = "GetName";
                Broadcast(msg, newclients.ElementAt<ClientInfo>(0));
               // Out.WriteLine("GetName");
            }
            newclients.Clear();
            Thread.Sleep(1);
        }
    }

    //остановка сервера
    ~Server()
    {
        //Если слушатель был создан
        if (listener != null)
        {
            listener.Stop();
            clients.Clear();
        }
        foreach (ClientInfo client in clients)
        {
            client.Client.Close();
        }
    }
}
class ClientInfo
{
    public string clientName;
    public Int32 id;
    public TcpClient Client;
    public List<byte> buffer = new List<byte>();
    public bool IsConnect;
    public bool IsReady = false;
    public int myTurnNumber;
    public Room room;
    public ClientInfo(TcpClient Client)
    {
        this.Client = Client;
        IsConnect = true;
    }
}
class Room
{
    public int maxPlayers = 4;
    public string name;
    public Int32 id;
    public int turn;
    public ClientInfo mainPlayer;
    public List<ClientInfo> players = new List<ClientInfo>();
    Random rnd = new Random();
    public double[,] res = new double[4, 5];
    public List<Builds> builds = new List<Builds>();

    //добавление всех клиентов в комнате для отправки запрашиваемому клиенту
    public string Players()
    {
        string msg = "|";
        for(int i = 0; i<players.Count; i++)
        {
            msg += players.ElementAt<ClientInfo>(i).clientName + "|";
        }
        return msg;
    }

    //проверка есть ли пользователь в комнате
    public ClientInfo IsHere(ClientInfo c)
    {
        for (int i = 0; i < players.Count; i++)
            if (players.ElementAt<ClientInfo>(i) == c)
                return players.ElementAt<ClientInfo>(i);
        return null;
        
    }

    //Создал два раза эти функции, потому что считаю, что их надо делать приватными, потому что можно какой-нить пидр может сможет через публичные продкасты отправлять во благо себе(читы)
    //рассылка всем пользователям
    private void Broadcast(string data, List<ClientInfo> cl)
    {
        foreach (ClientInfo sc in cl)
        {
            try
            {
                StreamWriter writer228 = new StreamWriter(sc.Client.GetStream());
                writer228.WriteLine(data);
                writer228.Flush();
            }
            catch (Exception e)
            {
            }
        }
    }

    //рассылка определённому пользователю
    private void Broadcast(string data, ClientInfo c)
    {
        List<ClientInfo> sc = new List<ClientInfo> { c };
        Broadcast(data, sc);
    }
    public void StartGame(ClientInfo c)
    {
        //рандомное назначение очерёдности ходов
        for (int i = 0; i < players.Count; i++)
        {
            players.ElementAt<ClientInfo>(i).myTurnNumber = i;
        }
        //Отправка игрокам в комнате, что игра началась
        foreach (ClientInfo client in players)
        {
            if (client.myTurnNumber == 0)
            {
                Broadcast("Start|start", client);
                turn = 1;
            }
            else
                Broadcast("Start", client);
            Builds build = new Builds();
            builds.Add(build);
        }
    }

    //Назначение игрока, который ходит
    public void EndTurn(ClientInfo c, string[] ress)
    {
        turn = c.myTurnNumber + 1;
        if (turn >= players.Count)
            turn = 0;
        foreach (ClientInfo client in players)
            if (client.myTurnNumber == turn)
            {
                Broadcast("Turn|", client);
            }
        for (int i = 0; i < builds.ElementAt<Builds>(c.myTurnNumber).rudnik.Count; i++)
        {
            if (builds.ElementAt<Builds>(c.myTurnNumber).rudnik.ElementAt<Rudnik>(i).spezialization == 1)
                res[c.myTurnNumber, 0] += 1;
            if (builds.ElementAt<Builds>(c.myTurnNumber).rudnik.ElementAt<Rudnik>(i).spezialization == 2)
                res[c.myTurnNumber, 1] += 1;
            if (builds.ElementAt<Builds>(c.myTurnNumber).rudnik.ElementAt<Rudnik>(i).spezialization == 3)
                res[c.myTurnNumber, 2] += 1;
        }
        for(int i = 0; i < builds.ElementAt<Builds>(c.myTurnNumber).windMills.Count; i++)
        {
            if (builds.ElementAt<Builds>(c.myTurnNumber).windMills.ElementAt<WindMill>(i).ambar == null || builds[c.myTurnNumber].windMills.ElementAt<WindMill>(i).bakery == null)
                continue;
            else
            {
                res[c.myTurnNumber, 3] += Math.Floor((double)builds[c.myTurnNumber].windMills.ElementAt<WindMill>(i).fields.Count / 4);
            }
        }
        for(int i = 0; i<builds[c.myTurnNumber].house.Count; i++)
        {
            if (!builds.ElementAt<Builds>(c.myTurnNumber).house.ElementAt<House>(i).isBuilt)
            {
                builds.ElementAt<Builds>(c.myTurnNumber).house.ElementAt<House>(i).isBuilt = true;
                if (builds.ElementAt<Builds>(c.myTurnNumber).house.ElementAt<House>(i).isFull)
                    res[c.myTurnNumber, 4] += 1;
            }
        }

        string msg = "Res|";
        msg += res[c.myTurnNumber, 0] + "|";
        msg += res[c.myTurnNumber, 1] + "|";
        msg += res[c.myTurnNumber, 2] + "|";
        msg += res[c.myTurnNumber, 3] + "|";
        msg += res[c.myTurnNumber, 4] + "|";

        Broadcast(msg, c);
    }

    //public void Res(ClientInfo c)
    //{
    //    string msg = "Res|";
    //    msg += res[c.myTurnNumber, 0] + "|";
    //    msg += res[c.myTurnNumber, 1] + "|";
    //    msg += res[c.myTurnNumber, 2] + "|";
    //    msg += res[c.myTurnNumber, 3] + "|";
    //    msg += res[c.myTurnNumber, 4] + "|";

    //    Broadcast(msg, c);
    //}
}
class Builds
{
    public List<WindMill> windMills = new List<WindMill>();
    public List<Rudnik> rudnik = new List<Rudnik>();
    public List<House> house = new List<House>();
    public List<MilitaryCamp> militaryCamp = new List<MilitaryCamp>();
    public List<Ambar> ambar = new List<Ambar>();
    public List<Bakery> bakery = new List<Bakery>();
    public List<Capital> caps = new List<Capital>();

    public List<Knight> knight = new List<Knight>();
}
//юниты
class Knight : Builds
{
    public ClientInfo owner;
    public MilitaryCamp mc;
    public int health = 1;

    //0) idWeapon;
    //1) idArmor;
    //2) idSheald;
    //3) horse;
    //4) pilum;
    //5) kopio;
    public int[] equpmnet = new int[6];
    public int[] stats = new int[4];
    //0) vsManAttack;
    //1) vsManDefens;
    //2) rangeAttack;
    //3) speed;

    public float x,z;
    public string name;
}

//здания
class WindMill : Builds
{
    public ClientInfo owner;
    public Bakery bakery;
    public Ambar ambar;
    public List<Field> fields = new List<Field>();
    public float x, z;
    public int health = 10;
    public string name;
}
class Bakery : Builds
{
    public ClientInfo owner;
    public float x, z;
    public int health = 10;
    public string name;
}
class Ambar : Builds
{
    public ClientInfo owner;
    public float x, z;
    public int health = 10;
    public string name;
}
class Field : Builds
{
    public ClientInfo owner;
    public float x, z;
}
class Rudnik : Builds
{
    public ClientInfo owner;
    public int spezialization;
    public float x, z;
    public int health = 10;
    public string name;
}
class House : Builds
{
    public ClientInfo owner;
    public bool isFull = false;
    public bool isBuilt = false;
    public float x, z;
    public int health = 10;
    public string name;
}
class MilitaryCamp : Builds
{
    public ClientInfo owner;
    public float x, z;
    public int countOfReserv = 0;
    public Knight unit;
    public bool isInside;
    public int health = 10;
    public string name;
}
class Capital
{
    public ClientInfo owner;
    public float x;
    public float y;
    public int health = 100;
    public string name;
}
