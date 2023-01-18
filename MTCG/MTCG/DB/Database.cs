using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MTCG.Cards;
using MTCG.User;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using static System.Net.Mime.MediaTypeNames;


namespace MTCG.DB
{
    public class Database : IDatabase
    {
        private readonly string _connectionString;
        private static NpgsqlConnection _conn;

        public Database()
        {
            _connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=password;Database=MTGCdb;";
            _conn = new NpgsqlConnection(_connectionString);
        }
        public Database(string connectionString)
        {
            _connectionString = connectionString;
            _conn = new NpgsqlConnection(_connectionString);
        }
        //helper for LoadUsers
        private void LoadStack(ConcurrentDictionary<string, User.User> users)
        {
            try
            {
                var sql = "SELECT * FROM card WHERE owner=@username AND deck=false AND trade=false";
                foreach (var user in users)
                {
                    using (var cmd = new NpgsqlCommand(sql, _conn))
                    {
                        cmd.Parameters.AddWithValue("@username", user.Key.ToString());
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var tmpCard = Card.CreateCard(
                                    reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                    reader.IsDBNull(0) ? "" : reader.GetString(0)
                                );
                                
                                users[user.Key].Stack.AddCard(tmpCard);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadUsers: " + e.Message);
            }
        }

        private void LoadDeck(ConcurrentDictionary<string, User.User> users)
        {
            try
            {
                var sql = "SELECT * FROM card WHERE owner=@username AND deck=true";
                foreach (var user in users)
                {
                    using (var cmd = new NpgsqlCommand(sql, _conn))
                    {
                        cmd.Parameters.AddWithValue("@username", user.Key.ToString());
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var tmpCard = Card.CreateCard(
                                    reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                    reader.IsDBNull(0) ? "" : reader.GetString(0)
                                );

                                users[user.Key].Deck.AddCard(tmpCard);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadUsers: " + e.Message);
            }
        }

        public ConcurrentDictionary<string, User.User> LoadUsers()
        {
            Console.WriteLine("LoadUSERS");
            ConcurrentDictionary<string, User.User> tmpusers = new ConcurrentDictionary<string, User.User>();
            try
            {
                var sql = "SELECT * FROM \"user\"";
                _conn.Open();
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Read the data from the reader and convert it to a User object
                            var tmpUser = new User.User
                            {
                                Username = reader.IsDBNull(0) ? "" : reader.GetString(0),
                                Password = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Token = reader.IsDBNull(0) ? "" : reader.GetString(1) + "-mtgcToken",
                                Name = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Bio = reader.IsDBNull(3) ? "" : reader.GetString(3),
                                Image = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                Coins = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                                Elo = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                                Wins = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                                Losses = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                                NumGames = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            };
                            tmpusers.TryAdd(tmpUser.Username, tmpUser);
                        }
                    }
                }
                LoadStack(tmpusers);
                LoadDeck(tmpusers);
                return tmpusers;
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadUsers: " + e.Message);
                return tmpusers;
            }
            finally
            {
                _conn.Close();
            }
        }
        public ConcurrentBag<string> LoadCardIDs()
        {
            Console.WriteLine("LoadCardIDS");
            ConcurrentBag<string> tmpCardIDs = new ConcurrentBag<string>();
            try
            {
                var sql = "SELECT \"id\" FROM card";
                _conn.Open();
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tmpCardIDs.Add(reader.GetString(0));
                        }
                    }
                }

                tmpCardIDs.TryPeek(out string result);
                Console.WriteLine("loda card ids:" +result);
                return tmpCardIDs;
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadCardIDs: " + e.Message);
                return tmpCardIDs;
            }
            finally
            {
                _conn.Close();
            }
        }
        //helper for LoadPackages()
        private ConcurrentBag<string> LoadPackageIDs()
        {
            Console.WriteLine("LoadPAckageIDs");
            ConcurrentBag<string> tmpPackageIDs = new ConcurrentBag<string>();
            try
            {
                var sql = "SELECT \"id\" FROM package";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tmpPackageIDs.Add(reader.GetString(0));
                            Console.WriteLine("package id:" + reader.GetString(0));
                        }
                    }
                }
                return tmpPackageIDs;
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadPackageIDs: " + e.Message);
                return tmpPackageIDs;
            }
        }
        public ConcurrentBag<Collection> LoadPackages()
        {
            Console.WriteLine("LoadPAckages");
            ConcurrentBag<Collection> tmpPackages = new ConcurrentBag<Collection>();
            _conn.Open();
            ConcurrentBag<string> tmpPackageIDs = new ConcurrentBag<string>();
            tmpPackageIDs = new ConcurrentBag<string>(LoadPackageIDs());
            try
            {
                foreach (var id in tmpPackageIDs)
                {
                    Collection tmppackage = new Collection(5);
                    var sqlCard = "SELECT * FROM card WHERE packageid=@id";
                    using (var cmd = new NpgsqlCommand(sqlCard, _conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var tmpCard = Card.CreateCard(
                                    reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                    reader.IsDBNull(0) ? null : reader.GetString(0)
                                    );
                                Console.WriteLine(tmpCard.Name);
                                Console.WriteLine(tmpCard.Damage);
                                Console.WriteLine(tmpCard.Id);
                                tmppackage.AddCard(tmpCard, true);
                            }
                        }
                    }
                    tmpPackages.Add(tmppackage);
                }

                return tmpPackages;
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadPackages: " + e.Message);
                return tmpPackages;
            }
            finally
            {
                _conn.Close();
            }

        }
        //save AllCards
        public ConcurrentDictionary<string, Cards.Card> LoadCards()
        {
            Console.WriteLine("LoadCards");
            ConcurrentDictionary<string, Cards.Card> tmpcards = new ConcurrentDictionary<string, Cards.Card>();
            try
            {
                var sql = "SELECT * FROM card";
                _conn.Open();
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Read the data from the reader and convert it to a User object
                            var tmpCard = Card.CreateCard(
                                reader.IsDBNull(1) ? "" : reader.GetString(1),
                                reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                reader.IsDBNull(0) ? "" : reader.GetString(0)
                            );
                            
                            tmpcards.TryAdd(tmpCard.Id.ToString(), tmpCard);
                        }
                    }
                }
                return tmpcards;
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadCards: " + e.Message);
                return tmpcards;
            }
            finally
            {
                _conn.Close();
            }
        }
        //Helper fpr LoadTradings
        private Card GetCard(ConcurrentDictionary<string, Cards.Card> cards, string cardID)
        {
            Console.WriteLine("GetCard");
            cards.TryGetValue(cardID, out Card value);
            return value;
        }
        public ConcurrentDictionary<string, Trading> LoadTradings(ConcurrentDictionary<string, Cards.Card> cards)
        {
            Console.WriteLine("LoadTradings");
            ConcurrentDictionary<string, Trading> tmpTradings = new ConcurrentDictionary<string, Trading>();
            try
            {
                var sql = "SELECT * FROM \"trade\"";
                _conn.Open();
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Read the data from the reader and convert it to a User object
                            var tmpTrade = new User.Trading(
                                reader.IsDBNull(0) ? null : reader.GetString(0),
                                GetCard(cards,reader.IsDBNull(1) ? null : reader.GetString(1)),
                                reader.IsDBNull(2) ? null : reader.GetString(2),
                                reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                reader.IsDBNull(4) ? null : reader.GetString(4)
                                );
                            tmpTradings.TryAdd(reader.IsDBNull(0) ? null : reader.GetString(0), tmpTrade);
                            Console.WriteLine(tmpTrade.Id);
                        }
                    }
                }
                return tmpTradings;
            }
            catch (Exception e)
            {
                Console.WriteLine("error LoadTradings: " + e.Message);
                return tmpTradings;
            }
            finally
            {
                _conn.Close();
            }
        }
        

        public bool CreateUser(User.User user)
        {
            try
            {
                var sql = "INSERT INTO \"user\" (username,password) VALUES (@username,@password)";
                using var cmd = new NpgsqlCommand(sql, _conn);
                cmd.Parameters.AddWithValue("username", user.Username);
                cmd.Parameters.AddWithValue("password", user.Password);

                _conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool UpdateUser(string username, string name, string bio, string image)
        {
            try
            {
                var sql = "UPDATE \"user\" SET name=@name,bio=@bio,image=@image WHERE username=@username ";
                using var cmd = new NpgsqlCommand(sql, _conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@bio", bio);
                cmd.Parameters.AddWithValue("@image", image);
                cmd.Parameters.AddWithValue("@username", username);

                _conn.Open();
                if (cmd.ExecuteNonQuery() == 1)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool CreatePackage(Collection package)
        {
            Guid packID = Guid.NewGuid();

            try
            {
                _conn.Open();
                using (var transaction = _conn.BeginTransaction())
                {
                    var sql1 = "INSERT INTO package (id) VALUES (@id)";
                    using (var cmd = new NpgsqlCommand(sql1, _conn))
                    {
                        cmd.Parameters.AddWithValue("@id", packID);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();
                    }

                    var sql2 = "INSERT INTO card (id,name,damage,element,type, packageid) " +
                               "VALUES (@id,@name,@damage,@element,@type,@packageid)";
                    

                    // Insert data into the table
                    using (var writer =
                           _conn.BeginBinaryImport(
                               "COPY card (id, name, damage, element, type, packageid) FROM STDIN (FORMAT BINARY)"))
                    {
                        // Add the data from the package.cards to the writer
                        foreach (var card in package.cards)
                        {
                            writer.StartRow();
                            writer.Write(card.Value.Id.ToString(), NpgsqlDbType.Varchar);
                            writer.Write(card.Value.Name, NpgsqlDbType.Varchar);
                            writer.Write(card.Value.Damage, NpgsqlDbType.Double);
                            writer.Write(card.Value.Element.ToString(), NpgsqlDbType.Varchar);
                            writer.Write(card.Value.GetCardType(), NpgsqlDbType.Varchar);
                            writer.Write(packID.ToString(), NpgsqlDbType.Varchar);
                        }
                        writer.Complete();
                    }
                    transaction.Commit();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        //BuyPackage
        public bool BuyPackage(string username, Collection package)
        {
            string packID;
            try
            {
                _conn.Open();
                var sql = "SELECT packageid FROM card WHERE  \"id\"=@id";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                { 
                    cmd.Parameters.AddWithValue("@id", package.cards.ElementAt(1).Key);
                    packID = (string)cmd.ExecuteScalar();
                }

                ChangeCardOwnerPackageID(username, packID);
                RemovePackage(packID);
                ChangeCoins(username);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool ChangeCardOwnerPackageID(string username, string packID)
        {
            try
            {
                var sql = "UPDATE card SET owner=@owner,packageid=NULL WHERE packageid=@packageid";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    cmd.Parameters.AddWithValue("@owner", username);
                    cmd.Parameters.AddWithValue("@packageID", packID);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }

        }

        public bool RemovePackage(string packID)
        {
            try
            {
                var sql = "DELETE from package WHERE id=@id";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    cmd.Parameters.AddWithValue("@id", packID);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
        }

        public bool ChangeCoins(string username)
        {
            try
            {
                var sql = "UPDATE \"user\" SET coins=coins-5 WHERE username=@username";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    Console.WriteLine("rows: " +cmd.ExecuteNonQuery());
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
        }

        //-----
        //ConfigureDeck
        public bool RemoveDeck(string username)
        {
            try
            {
                _conn.Open();
                var sql = "UPDATE card SET deck=false WHERE owner=@owner";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    cmd.Parameters.AddWithValue("@owner", username);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool AddDeck(string username, string cardID)
        {
            try
            {
                _conn.Open();
                var sql = "UPDATE card SET deck=true WHERE owner=@owner AND id=@id";
                using (var cmd = new NpgsqlCommand(sql, _conn))
                {
                    cmd.Parameters.AddWithValue("@owner", username);
                    cmd.Parameters.AddWithValue("@id", cardID);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }
        //-----

        public bool CreateTrade(string tradeID, string cardID, string type, double damage, string username)
        {
            try
            {
                _conn.Open();
                using (var trans = _conn.BeginTransaction())
                {
                    var sql = "INSERT INTO trade (\"id\", \"cardToTrade\", type, damage, owner) VALUES (@id,@cardToTrade,@type,@damage,@owner)";
                    using (var cmd = new NpgsqlCommand(sql, _conn))
                    {
                        cmd.Parameters.AddWithValue("@id", tradeID);
                        cmd.Parameters.AddWithValue("@cardToTrade", cardID);
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@damage", damage);
                        cmd.Parameters.AddWithValue("@owner", username);

                        cmd.ExecuteNonQuery();
                    }

                    var sql2 = "UPDATE card SET trade=true WHERE \"id\"=@id";
                    using (var cmd2 = new NpgsqlCommand(sql2, _conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", cardID);
                        cmd2.ExecuteNonQuery();
                    }
                    trans.Commit();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool DeleteTrade(string tradeID, string cardID)
        {
            try
            {
                _conn.Open();
                using (var trans = _conn.BeginTransaction())
                {
                    var sql = "DELETE from trade WHERE id=@id";
                    using (var cmd = new NpgsqlCommand(sql, _conn))
                    {
                        cmd.Parameters.AddWithValue("@id", tradeID);
                        cmd.ExecuteNonQuery();
                    }

                    var sql2 = "UPDATE card SET trade=false WHERE \"id\"=@id";
                    using (var cmd2 = new NpgsqlCommand(sql2, _conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", cardID);
                        cmd2.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool PerformTrade(string tradeID, string cardID1, string cardID2, string username1, string username2)
        {
            try
            {
                _conn.Open();
                using (var trans = _conn.BeginTransaction())
                {
                    var sql = "DELETE from trade WHERE id=@id";
                    using (var cmd = new NpgsqlCommand(sql, _conn))
                    {
                        cmd.Parameters.AddWithValue("@id", tradeID);
                        cmd.ExecuteNonQuery();
                    }

                    var sql2 = "UPDATE card SET trade=false,owner=@owner WHERE \"id\"=@id";
                    using (var cmd2 = new NpgsqlCommand(sql2, _conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", cardID1);
                        cmd2.Parameters.AddWithValue("@owner", username2);
                        cmd2.ExecuteNonQuery();
                    }

                    var sql3 = "UPDATE card SET owner=@owner WHERE \"id\"=@id";
                    using (var cmd2 = new NpgsqlCommand(sql2, _conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", cardID2);
                        cmd2.Parameters.AddWithValue("@owner", username1);
                        cmd2.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }

        public bool Battle(User.User user1, User.User user2, string log)
        {
            try
            {
                _conn.Open();
                using (var trans = _conn.BeginTransaction())
                {
                    var sql1 = "INSERT INTO battle (player1,player2,battlelog) VALUES (@player1,@player2,@battlelog)";
                    using (var cmd = new NpgsqlCommand(sql1, _conn))
                    {
                        cmd.Parameters.AddWithValue("@player1", user1.Username);
                        cmd.Parameters.AddWithValue("@player2", user2.Username);
                        cmd.Parameters.AddWithValue("@battlelog", log);
                        cmd.ExecuteNonQuery();
                    }

                    var sql2 = "UPDATE \"user\" SET \"elo\"=@elo,\"wins\"=@wins,\"losses\"=@losses,\"numGames\"=@numGames WHERE \"username\"=@user";
                    using (var cmd = new NpgsqlCommand(sql2, _conn))
                    {
                        cmd.Parameters.AddWithValue("@elo", user1.Elo.Value);
                        cmd.Parameters.AddWithValue("@wins", user1.Wins.Value);
                        cmd.Parameters.AddWithValue("@losses", user1.Losses.Value);
                        cmd.Parameters.AddWithValue("@numGames", user1.NumGames.Value);
                        cmd.Parameters.AddWithValue("@user", user1.Username);
                        cmd.ExecuteNonQuery();
                    }

                    var sql3 = "UPDATE \"user\" SET \"elo\"=@elo,\"wins\"=@wins,\"losses\"=@losses,\"numGames\"=@numGames WHERE \"username\"=@user";
                    using (var cmd = new NpgsqlCommand(sql3, _conn))
                    {
                        cmd.Parameters.AddWithValue("@elo", user2.Elo.Value);
                        cmd.Parameters.AddWithValue("@wins", user2.Wins.Value);
                        cmd.Parameters.AddWithValue("@losses", user2.Losses.Value);
                        cmd.Parameters.AddWithValue("@numGames", user2.NumGames.Value);
                        cmd.Parameters.AddWithValue("@user", user2.Username);
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
                return false;
            }
            finally
            {
                _conn.Close();
            }
        }
    }
}
