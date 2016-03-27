namespace BunnyWars.Core
{
    using System;
    using System.Collections.Generic;
    using Wintellect.PowerCollections;

    public class Room : IComparable<Room>
    {
        public Room(int id)
        {
            this.Id = id;
            this.BunniesByTeam = new OrderedSet<Bunny>[5];
        }

        public int Id { get; set; }

        public OrderedSet<Bunny>[] BunniesByTeam { get; set; }

        public int CompareTo(Room other)
        {
            return this.Id.CompareTo(other.Id);
        }
    }
}