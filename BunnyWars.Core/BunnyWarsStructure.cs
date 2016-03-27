namespace BunnyWars.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Wintellect.PowerCollections;

    public class BunnyWarsStructure : IBunnyWarsStructure
    {
        private readonly OrderedSet<Room> allRooms;
        private readonly OrderedDictionary<int, Room> roomsById;
        private readonly OrderedDictionary<string, Bunny> bunniesByName;
        private readonly SortedSet<Bunny>[] bunniesByTeam;
        private readonly OrderedDictionary<string, Bunny> suffixBunnies;

        public BunnyWarsStructure()
        {
            this.allRooms = new OrderedSet<Room>();
            this.roomsById = new OrderedDictionary<int, Room>();
            this.bunniesByName = new OrderedDictionary<string, Bunny>();
            this.bunniesByTeam = new SortedSet<Bunny>[5];
            this.suffixBunnies = new OrderedDictionary<string, Bunny>(StringComparer.Ordinal);
        }

        public int BunnyCount { get; private set; }

        public int RoomCount { get; private set; }

        public void AddRoom(int roomId)
        {
            if (this.roomsById.ContainsKey(roomId))
            {
                throw new ArgumentException("Room exists");
            }
           
            var room = new Room(roomId);
            this.roomsById.Add(roomId, room);
            this.allRooms.Add(room);
            this.RoomCount++;
        }

        public void AddBunny(string name, int team, int roomId)
        {
            if (this.bunniesByName.ContainsKey(name) ||
                !this.roomsById.ContainsKey(roomId))
            {
                throw new ArgumentException("Room does not exists");
            }

            var bunny = new Bunny(name, team, roomId);
            var room = this.roomsById[roomId];
            if (room.BunniesByTeam[team] == null)
            {
                room.BunniesByTeam[team] = new OrderedSet<Bunny>();
            }

            room.BunniesByTeam[team].Add(bunny);
            this.bunniesByName.Add(name, bunny);
            if (this.bunniesByTeam[team] == null)
            {
                this.bunniesByTeam[team] = new SortedSet<Bunny>();
            }

            this.bunniesByTeam[team].Add(bunny);
            var nameReversed = new string(name.Reverse().ToArray());
            this.suffixBunnies.Add(nameReversed, bunny);
            this.BunnyCount++;
        }

        public void Remove(int roomId)
        {
            if (!this.roomsById.ContainsKey(roomId))
            {
                throw new ArgumentException("Room does not exists");
            }

            var roomTobeRemoved = this.roomsById[roomId];
            
            foreach (var team in roomTobeRemoved.BunniesByTeam)
            {
                if (team != null)
                {
                    foreach (var bunny in team)
                    {
                        this.bunniesByTeam[bunny.Team].Remove(bunny);
                        this.bunniesByName.Remove(bunny.Name);

                        this.BunnyCount--;
                    }
                }
            }
            
            this.allRooms.Remove(roomTobeRemoved);
            this.roomsById.Remove(roomId);
            this.RoomCount--;
        }

        public void Next(string bunnyName)
        {
            if (!this.bunniesByName.ContainsKey(bunnyName))
            {
                throw new ArgumentException("Bunnie does not exists");
            }

            var bunny = this.bunniesByName[bunnyName];
            var currentRoom = this.roomsById[bunny.RoomId];

            var currentRoomIndex = this.allRooms.IndexOf(currentRoom);
            if (currentRoomIndex >= this.RoomCount - 1)
            {
                var nextRoom = this.allRooms[0];
                currentRoom.BunniesByTeam[bunny.Team].Remove(bunny);

                if (nextRoom.BunniesByTeam[bunny.Team] == null)
                {
                    nextRoom.BunniesByTeam[bunny.Team] = new OrderedSet<Bunny>();
                }

                nextRoom.BunniesByTeam[bunny.Team].Add(bunny);
                bunny.RoomId = nextRoom.Id;
            }
            else
            {
                var nextRoomIndex = currentRoomIndex + 1;
                var nextRoom = this.allRooms[nextRoomIndex];
                currentRoom.BunniesByTeam[bunny.Team].Remove(bunny);
                
                if (nextRoom.BunniesByTeam[bunny.Team] == null)
                {
                    nextRoom.BunniesByTeam[bunny.Team] = new OrderedSet<Bunny>();
                }

                nextRoom.BunniesByTeam[bunny.Team].Add(bunny);
                bunny.RoomId = nextRoom.Id;
            }
        }

        public void Previous(string bunnyName)
        {
            if (!this.bunniesByName.ContainsKey(bunnyName))
            {
                throw new ArgumentException("Bunnie does not exists");
            }

            var bunny = this.bunniesByName[bunnyName];
            var currentRoom = this.roomsById[bunny.RoomId];
            var currentRoomIndex = this.allRooms.IndexOf(currentRoom);
            if (currentRoomIndex == 0)
            {
                var prevRoom = this.allRooms[this.RoomCount - 1];
                currentRoom.BunniesByTeam[bunny.Team].Remove(bunny);
                if (prevRoom.BunniesByTeam[bunny.Team] == null)
                {
                    prevRoom.BunniesByTeam[bunny.Team] = new OrderedSet<Bunny>();
                }

                prevRoom.BunniesByTeam[bunny.Team].Add(bunny);
                bunny.RoomId = prevRoom.Id;
            }
            else
            {
                var prevRoomIndex = currentRoomIndex - 1;
                var prevRoom = this.allRooms[prevRoomIndex];
                currentRoom.BunniesByTeam[bunny.Team].Remove(bunny);
                if (prevRoom.BunniesByTeam[bunny.Team] == null)
                {
                    prevRoom.BunniesByTeam[bunny.Team] = new OrderedSet<Bunny>();
                }

                prevRoom.BunniesByTeam[bunny.Team].Add(bunny);
                bunny.RoomId = prevRoom.Id;
            }
        }

        public void Detonate(string bunnyName)
        {
            if (!this.bunniesByName.ContainsKey(bunnyName))
            {
                throw new ArgumentException("Bunny does not exists");
            }

            var bunnyToExplode = this.bunniesByName[bunnyName];
            var roomThatWillExplode = this.roomsById[bunnyToExplode.RoomId];
            var deadBunnies = new List<Bunny>();

            for (int i = 0; i < roomThatWillExplode.BunniesByTeam.Length; i++)
            {
                if (roomThatWillExplode.BunniesByTeam[i] != null &&
                    i != bunnyToExplode.Team)
                {
                    foreach (var bunny in roomThatWillExplode.BunniesByTeam[i])
                    {
                        bunny.Health -= 30;
                        if (bunny.Health <= 0)
                        {
                            bunnyToExplode.Score++;
                            deadBunnies.Add(bunny);
                        }
                    }
                }
            }

            foreach (var deadBunny in deadBunnies)
            {
                this.BunnyCount--;
                this.bunniesByName.Remove(deadBunny.Name);
                this.bunniesByTeam[deadBunny.Team].Remove(deadBunny);
                roomThatWillExplode.BunniesByTeam[deadBunny.Team].Remove(deadBunny);
            }
        }

        public IEnumerable<Bunny> ListBunniesByTeam(int team)
        {
            var res = this.bunniesByTeam[team];

            return res;
        }

        public IEnumerable<Bunny> ListBunniesBySuffix(string suffix)
        {
            var reSuffix = new string(suffix.Reverse().ToArray());
            return this.suffixBunnies.Range(reSuffix, true, reSuffix + char.MaxValue, true).Values;
        }
    }
}