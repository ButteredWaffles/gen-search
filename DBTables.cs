using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Gensearch
{
    [Table("Items")]
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [Unique]
        public string item_name {get; set;}
        public string description {get; set;}
        public int rarity {get; set;}
        public int max_stack {get; set;}
        public int sell_price {get; set;}
        public string combinations {get; set;}

        public override bool Equals(object obj)
        {
            Item other = (Item) obj; 
            return item_name == other.item_name && description == other.description
            && rarity == other.rarity && max_stack == other.max_stack 
            && sell_price == other.sell_price && combinations == other.combinations;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }

    [Table("Monsters")]
    public class Monster {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [Unique]
        public string mon_name {get; set;}
        public int base_hp {get; set;}
        public double base_size {get; set;}
        public double small_size {get; set;}
        public double silver_size {get; set;}
        public double king_size {get; set;}
    }

    [Table("MonsterParts")]
    public class MonsterPart {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        public string part_name {get; set;}
        public int stagger_value {get; set;}
        public string extract_color {get; set;}

        [ForeignKey(typeof(Monster))]
        public int monsterid {get; set;}
        [OneToOne]
        public Monster monster {get; set;}
    }

    [Table("MonsterDrops")]
    public class MonsterDrop {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [ForeignKey(typeof(Item))]
        public int itemid {get; set;}
        [ForeignKey(typeof(Monster))]
        public int monsterid {get; set;}
        [ForeignKey(typeof(MonsterPart))]
        public int sourceid {get; set;}
        public string rank {get; set;}
        public int drop_chance {get; set;}
        public int quantity {get; set;}

        [OneToOne]
        public Item item {get; set;}
        [OneToOne]
        public Monster monster {get; set;}
        [OneToOne]
        public MonsterPart source {get; set;}
    }

    [Table("Quests")]
    public class Quest {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [Unique]
        public string quest_name {get; set;}
        public string quest_type {get; set;} // hunt, gather, capture, slay, survive
        public string quest_description {get; set;}
        public string isKey {get; set;}
        public string isProwler {get; set;}
        public int timeLimit {get; set;}
        public int contractFee {get; set;}

        [ForeignKey(typeof(Goal))]
        public int goalid {get; set;}
        [ForeignKey(typeof(Goal))]
        public int subgoalid {get; set;}

        [OneToOne]
        public Goal goal {get; set;}
        [OneToOne]
        public Goal subgoal {get; set;}
    }

    [Table("QuestGoals")]
    public class Goal {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        public int zenny_reward {get; set;}
        public int hrp_reward {get; set;}
        public int wycadpts_reward {get; set;}
        public string goal_description {get; set;}
    }

    [Table("QuestMonsters")]
    public class QuestMonster {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        [ForeignKey(typeof(Quest))]
        public int questid {get; set;}
        [ForeignKey(typeof(Monster))]
        public int monsterid {get; set;}
        public int amount {get; set;}
        public string isIntruder {get; set;}
        public int mon_hp {get; set;}
        public double stag_multiplier {get; set;}
        public double atk_multiplier {get; set;}
        public double def_multiplier {get; set;}
        public double exh_multiplier {get; set;}
        public double diz_multiplier {get; set;}
        public double mnt_multiplier {get; set;}

        [OneToOne]
        public Monster monster {get; set;}
        [OneToOne]
        public Quest quest {get; set;}
    }

    [Table("QuestBoxItems")]
    public class QuestBoxItem {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        public string box_type {get; set;} // main reward A, main reward B, supplies, etc...
        [ForeignKey(typeof(Quest))]
        public int questid {get; set;}
        [ForeignKey(typeof(Item))]
        public int itemid {get; set;}
        public int quantity {get; set;}
        public double appear_chance {get; set;}

        [OneToOne]
        public Quest quest {get; set;}
        [OneToOne]
        public Item item {get; set;}
    }

    [Table("QuestUnlocks")]
    public class QuestUnlock {
        [PrimaryKey, AutoIncrement]
        public int id {get; set;}
        public string unlock_type {get; set;} // whether this quest is a prereq or unlocks another one
        [ForeignKey(typeof(Quest))]
        public int questid {get; set;}

        [OneToOne]
        public Quest quest {get; set;}
    }
}