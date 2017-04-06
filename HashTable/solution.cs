///
/// Author: Thomas LaSalle (tel5027)
/// Class: CSCI 541-03
/// Date: February 16, 2017
/// 

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIT_CS
{
    /// <summary>
    /// An exception used to indicate a problem with how
    /// a HashTable instance is being accessed
    /// </summary>
    public class NonExistentKey<Key> : Exception
    {
        /// <summary>
        /// The key that caused this exception to be raised
        /// </summary>
        public Key BadKey { get; private set; }

        /// <summary>
        /// Create a new instance and save the key that
        /// caused the problem.
        /// </summary>
        /// <param name="k">
        /// The key that was not found in the hash table
        /// </param>
        public NonExistentKey(Key k) :
            base("Non existent key in HashTable: " + k)
        {
            BadKey = k;
        }

    }

    /// <summary>
    /// An associative (key-value) data structure.
    /// A given key may not appear more than once in the table,
    /// but multiple keys may have the same value associated with them.
    /// Tables are assumed to be of limited size are expected to automatically
    /// expand if too many entries are put in them.
    /// </summary>
    /// <param name="Key">the types of the table's keys (uses Equals())</param>
    /// <param name="Value">the types of the table's values</param>
    interface Table<Key, Value> : IEnumerable<Key>
    {
        /// <summary>
        /// Add a new entry in the hash table. If an entry with the
        /// given key already exists, it is replaced without error.
        /// put() always succeeds.
        /// (Details left to implementing classes.)
        /// </summary>
        /// <param name="k">the key for the new or existing entry</param>
        /// <param name="v">the (new) value for the key</param>
        void Put(Key k, Value v);

        /// <summary>
        /// Does an entry with the given key exist?
        /// </summary>
        /// <param name="k">the key being sought</param>
        /// <returns>true iff the key exists in the table</returns>
        bool Contains(Key k);

        /// <summary>
        /// Fetch the value associated with the given key.
        /// </summary>
        /// <param name="k">The key to be looked up in the table</param>
        /// <returns>the value associated with the given key</returns>
        /// <exception cref="NonExistentKey">if Contains(key) is false</exception>
        Value Get(Key k);
    }

    class TableFactory {
        /// <summary>
        /// Create a Table.
        /// (The student is to put a line of code in this method corresponding
        /// to the name of the Table implementor s/he has designed.)
        /// </summary>
        /// <param name="K">the key type</param>
        /// <param name="V">the value type</param>
        /// <param name="capacity">The initial maximum size of the table</param>
        /// <param name="loadThreshold">
        /// The fraction of the table's capacity that when
        /// reached will cause a rebuild of the table to a 50% larger size
        /// </param>
        /// <returns>A new instance of Table</returns>
        public static Table<K, V> Make<K, V>
            (int capacity = 100, double loadThreshold = 0.75) {
            return new LinkedHashTable<K, V>(capacity, loadThreshold);
        }
    }

    public class Entry<K, V>
    {
        /// <summary>
        /// An entry in the HashTable
        /// </summary>
        /// <param name="K">the key type</param>
        /// <param name="V"> the value type</param>
        /// <param name="k">the key</param>
        /// <param name="v">the value</param>
        public K key { get; set; }
        public V value { get; set; }

        public Entry(K k, V v)
        {
            key = k;
            value = v;
        }
    }

    class LinkedHashTable<K, V> : Table<K, V>
    {
        /// <summary>
        /// The implementation of a LinkedHashTable, which can
        /// store Key/Value pairs in buckets based on an Entry's
        /// HashCode
        /// </summary>

        List<List<Entry<K, V>>> entries;

        int capacity;
        int size = 0;
        double loadThreshold;

        public LinkedHashTable(int cap, double load) {
            capacity = cap;
            loadThreshold = load;

            entries = new List<List<Entry<K, V>>>(cap);

            for (int i = 0; i < cap; i++)
            {
                entries.Add(new List<Entry<K, V>>());
            }
        }

        /// <summary>
		/// Add a new entry in the hash table. If an entry with the
		/// given key already exists, it is replaced without error.
		/// put() always succeeds.
		/// (Details left to implementing classes.)
		/// </summary>
		/// <param name="k">the key for the new or existing entry</param>
		/// <param name="v">the (new) value for the key</param>
        public void Put(K key, V value)
        {
            Entry<K, V> e = new Entry<K, V>(key, value);
            int index = (e.GetHashCode() % capacity);

            ///If a key doesn't exist, add it with it's value
            if (!Contains(key))
            {
                List<Entry<K, V>> bucket = entries[index];
                bucket.Add(e);
                entries[index] = bucket;

                size++;                
            }
            else
            {
                ///If it does exist, replace its value with the new one

                foreach (List<Entry<K, V>> entryLists in entries)
                {
                    foreach (Entry<K, V> entry in entryLists)
                    {
                        if (entry.key.Equals(key))
                        {
                            entry.value = value;
                        }
                    }
                }
            }

            if (size == (capacity * loadThreshold)) ReHash();
        }

        /// <summary>
        /// ReHash will expand the HashTree size, and ReHash
        /// the HashTable to replace each entry into a new spot
        /// in the table
        /// </summary>
        public void ReHash()
        {
            capacity = (int)(capacity + (capacity * loadThreshold));
            List<List<Entry<K, V>>> entrcopy = entries;
            entries = new List<List<Entry<K, V>>>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                entries.Add(new List<Entry<K, V>>());
            }

            foreach(List<Entry<K,V>> entrlist in entrcopy)
            {
                foreach(Entry<K,V> entry in entrlist)
                {
                    Put(entry.key, entry.value);
                }
            }
        }

        /// <summary>
		/// Does an entry with the given key exist?
		/// </summary>
		/// <param name="k">the key being sought</param>
		/// <returns>true iff the key exists in the table</returns>
        public bool Contains(K key)
        {
            foreach (List<Entry<K, V>> entryLists in entries)
            {
                foreach (Entry<K, V> entry in entryLists)
                {
                    if (entry.key.Equals(key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
		/// Fetch the value associated with the given key.
		/// </summary>
		/// <param name="k">The key to be looked up in the table</param>
		/// <returns>the value associated with the given key</returns>
		/// <exception cref="NonExistentKey">if Contains(key) is false</exception>
        public V Get(K key)
        {
            if (Contains(key)) {
                foreach (List<Entry<K, V>> entryLists in entries)
                {
                    foreach (Entry<K, V> entry in entryLists)
                    {
                        if (entry.key.Equals(key))
                        {
                            return entry.value;
                        }
                    }
                }
            }

            throw new NonExistentKey<K>(key);
        }

        /// <summary>
        /// Retrieve a generic Iterator/Enumerator for the HashTable
        /// </summary>
        /// <returns>The Iterator/Enumerator</returns>
        IEnumerator<K> IEnumerable<K>.GetEnumerator()
        {
            foreach (List<Entry<K, V>> entryLists in entries)
            {
                foreach (Entry<K, V> entry in entryLists)
                {
                    K key = entry.key;
                    yield return key;
                }
            }
        }

        /// <summary>
        /// Retrieve an Iterator/Enumerator for the HashTable
        /// </summary>
        /// <returns>The Iterator/Enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return entries.GetEnumerator();
        }
    }

    class TestTable{
        ///<summary>
        /// Unit Test class for the LinkedHashTable implementation
        ///</summary>

        public static void test()
        {
            try
            {
                Console.WriteLine("=========================");
                Console.WriteLine("=========TEST 1==========");
                Console.WriteLine("=========================");

                ///Unique keys test. Tom, 1 and Sam, 12 should have their values
                ///updated to 9 and 21 respectfully.
                Table<String, int> t1 = TableFactory.Make<String, int>(10, 0.5);
                t1.Put("Tom", 1);
                t1.Put("Casey", 2);
                t1.Put("Adam", 3);
                t1.Put("James", 4);
                t1.Put("Erik", 5);
                t1.Put("Dan", 6);
                t1.Put("Jeremy", 7);
                t1.Put("Bill", 8);
                t1.Put("Tom", 9);
                t1.Put("Kenny", 10);
                t1.Put("David", 11);
                t1.Put("Sam", 12);
                t1.Put("Robert", 13);
                t1.Put("Lisa", 14);
                t1.Put("Margaret", 15);
                t1.Put("Della", 16);
                t1.Put("Marc", 17);
                t1.Put("Tiffany", 18);
                t1.Put("Kori", 19);
                t1.Put("Jack", 20);
                t1.Put("Sam", 21);

                foreach (String s in t1)
                {
                    Console.WriteLine(s + " -> " + t1.Get(s));
                }
            }
            catch (NonExistentKey<String> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }

            Console.WriteLine(" ");

            try
            {
                Console.WriteLine("=========================");
                Console.WriteLine("=========TEST 2==========");
                Console.WriteLine("=========================");

                //Large test, Tests behavior for a large sized HashTree.
                //Start with initial capacity of 100, Continue to rehash until
                //2000 elements are contained in the HashTree
                Table<Guid, String> t1 = TableFactory.Make<Guid, String>(100, 0.5);
                for(int i = 0; i < 2000; i++)
                {
                    Guid g = Guid.NewGuid();
                    t1.Put(g, g.ToString());
                }

                foreach (Guid g in t1)
                {
                    Console.WriteLine(g + " -> " + t1.Get(g));
                }
            }
            catch (NonExistentKey<String> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }

            Console.WriteLine(" ");

            try
            {
                Console.WriteLine("=========================");
                Console.WriteLine("=========TEST 3==========");
                Console.WriteLine("=========================");

                ///Contains test. Tests the contains function to see if the table contains
                ///the given key
                Table<String, bool> t1 = TableFactory.Make<String, bool>(20, 0.5);
                t1.Put("I'm a boy", true);
                t1.Put("Candy is healthy", false);
                t1.Put("This class is awesome", true);
                t1.Put("I'm a brown noser", true);
                t1.Put("F grade is passing", false);
                t1.Put("This class is difficult", false);
                t1.Put("I enjoyed this project", true);
                t1.Put("I love RIT Hockey", true);
                t1.Put("Writing unit tests is hard", true);
                t1.Put("I can do it!", true);
                t1.Put("I'm taking 7 classes this semester", false);
                t1.Put("ReHash() works properly", true);
                t1.Put("Prof. Brown likes video games", true);
                t1.Put("Latest C# Version is 4.5", false);

                if (t1.Contains("I can do it!"))
                {
                    Console.WriteLine("Key: I can do it! is in the table");
                }
                else
                {
                    Console.WriteLine("Key: I can do it! is not in the table");
                }

                if (t1.Contains("Books are expensive!"))
                {
                    Console.WriteLine("Key: Books are expensive! is in the table");
                }
                else
                {
                    Console.WriteLine("Key: Books are expensive! is not in the table");
                }

                if (t1.Contains("Writing unit tests is hard"))
                {
                    Console.WriteLine("Key: Writing unit tests is hard is in the table");
                }
                else
                {
                    Console.WriteLine("Key: Writing unit tests is hard is not in the table");
                }

                if (t1.Contains("I'm a scary dude!"))
                {
                    Console.WriteLine("Key: I'm a scary dude! is in the table");
                }
                else
                {
                    Console.WriteLine("Key: I'm a scary dude! is not in the table");
                }

                Console.WriteLine("=========================");

                foreach (String s in t1)
                {
                    Console.WriteLine(s + " -> " + t1.Get(s));
                }

                Console.WriteLine("=========================");
            }
            catch (NonExistentKey<String> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }

            Console.WriteLine(" ");
            Console.WriteLine("=========================");
            Console.WriteLine("=========TEST 4==========");
            Console.WriteLine("=========================");
        }
    }    
	
	class MainClass
	{
		public static void Main( string[] args )
		{
            ///Three unit tests written by me
            TestTable.test();

            ///Provided tests. Tests Put and Get methods, as well as the
            ///NonExistantKey exception
			Table< String, String> ht = TableFactory.Make< String, String >( 4, 0.5 );
			ht.Put( "Joe", "Doe" );
			ht.Put( "Jane", "Brain" );
			ht.Put( "Chris", "Swiss" );
			try
			{
				foreach ( String first in ht )
				{
					Console.WriteLine( first + " -> " + ht.Get( first ) );
				}
				Console.WriteLine( "=========================" );
				
				ht.Put( "Wavy", "Gravy" );
				ht.Put( "Chris", "Bliss" );
				foreach ( String first in ht )
				{
					Console.WriteLine( first + " -> " + ht.Get( first ) );
				}
				Console.WriteLine( "=========================" );
				
				Console.Write( "Jane -> " );
				Console.WriteLine( ht.Get( "Jane" ) );
				Console.Write( "John -> " );
				Console.WriteLine( ht.Get( "John" ) );
			}
			catch ( NonExistentKey< String > nek )
			{
				Console.WriteLine( nek.Message );
				Console.WriteLine( nek.StackTrace );
			}

			Console.ReadLine();
		}
	}
}

