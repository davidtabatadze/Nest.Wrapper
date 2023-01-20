using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Nest.Wrapper.Test
{
    class Program
    {

        class TestEntity : ElasticEntity<DateTime>
        {

        }
        class TestEntityLong : ElasticEntity<long>
        {
            public string Name { get; set; }
            public DateTime? Date { get; set; }
        }
        class TestEntityString : ElasticEntity<string>
        {
            public string Name { get; set; }
            public DateTime? Date { get; set; }
        }

        class Dodolina : ElasticEntity<long>
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public bool Ok { get; set; }
            public Dictionary<string, string> Dict { get; set; }
            public dynamic Obj { get; set; }
        }

        class SomeGeneric
        {
            public string Name { get; set; }
        }
        class TestEntityGeneric
        {
            public long Id { get; set; }
        }
        class TestEntityGeneric<T> : TestEntityGeneric, IElasticEntityKeyable<long>
        {
            public List<T> Data { get; set; }
        }

        public class RuleCleanerModel : IElasticEntityKeyable<string>
        {
            public string Id { get; set; }
            public string Kind { get; set; }

        }

        static string Check(bool result)
        {
            return result ? "=OK= " : "!!!! ";
        }



        static async Task Main(string[] args)
        {
            try
            {
                var elastic = new ElasticContext(
                    new ElasticConfiguration
                    {
                        Nodes = new List<string> { "http://development.elasticsearch-01.private.optio.ai:9200/" }
                    }
                    //new Dictionary<string, Type>
                    //{
                    //    { "datiko-test-longs", typeof(TestEntityLong) },
                    //    { "datiko-test-strings", typeof(TestEntityString) }
                    //}
                );

                //elastic.SetMappings(
                //    new Dictionary<string, Type>
                //    {
                //        { "datiko-test-longs", typeof(TestEntityLong) },
                //        { "datiko-test-strings", typeof(TestEntityString) },
                //        //{ "datiko-test-generic", typeof(TestEntityGeneric) },


                //        { "datiko.dodolina", typeof(Dodolina) }
                //    }
                //);

                //elastic.ConfigureIndices(new List<ElasticIndexConfiguration>
                //{
                //    new ElasticIndexConfiguration{

                //        Name = "datiko.dodolina",
                //        ModelType = typeof(Dodolina),
                //        Configuration = index => index
                //                            .Settings(s=>s.NumberOfReplicas(0).NumberOfShards(1))
                //                            .Map<Dodolina>(m=>m
                //                                .Properties(p=>p
                //                                    .Keyword(k=>k.Name(n=>n.Id))
                //                                    .Keyword(k=>k.Name(n=>n.Name))
                //                                    .Date(k=>k.Name(n=>n.Date))
                //                                    .Nested<Dictionary<string, string>>(k=>k.Name(n=>n.Dict))
                //                                    .Nested<dynamic>(k=>k.Name(n=>n.Obj))
                //                                )
                //                            )

                //    }
                //});

                var connection = elastic.Connection;

                await elastic.Insert(new List<Dodolina> { });

                //await elastic.Insert(new Dodolina
                //{
                //    Id = new Random().Next(1, 1000000),
                //    Name = "lina",
                //    Date = DateTime.Now.AddDays(999),
                //    Ok = true,
                //    Dict = new Dictionary<string, string> { { "foo", "bar" } },
                //    Obj = new { a = 1, b = "2" }
                //});

                await elastic.Delete<Dodolina>();

                //await elastic.Insert(new TestEntityGeneric<SomeGeneric> { Id = 999, Data = new List<SomeGeneric> { } });
                
                var x = 0;


                await elastic.Get<Dodolina>(777);


                await elastic.Delete<TestEntityLong>();
                await elastic.Delete<TestEntityString>();

                var dt = DateTime.Now;

                var entityLong777 = new TestEntityLong
                {
                    Id = 777,
                    Date = dt,
                    Name = "datiko777"
                };
                var entityLong888 = new TestEntityLong
                {
                    Id = 888,
                    Date = dt,
                    Name = "datiko888"
                };
                var entityLong999 = new TestEntityLong
                {
                    Id = 999,
                    Date = dt,
                    Name = "datiko999"
                };

                var entityString111 = new TestEntityString
                {
                    Id = "111",
                    Date = dt,
                    Name = "datiko111"
                };
                var entityString222 = new TestEntityString
                {
                    Id = "222",
                    Date = dt,
                    Name = "datiko222"
                };
                var entityString333 = new TestEntityString
                {
                    Id = "333",
                    Date = dt,
                    Name = "datiko333"
                };


                Console.WriteLine("Checks ...\n\n");
                // ***mappings
                var ekeys = false;
                try
                {
                    var mapInvalid = new ElasticContext(
                        new ElasticConfiguration
                        {
                            Nodes = new List<string> { "http://development.elasticsearch-01.private.optio.ai:9200/" }
                        },
                        new Dictionary<string, Type>
                        {
                            { "datiko-test-longs", typeof(TestEntityLong) },
                            { "datiko-test-strings", typeof(TestEntityString) },
                            { "datiko-test", typeof(TestEntity) }
                        }
                    );
                }
                catch (Exception e)
                {
                    ekeys = true;
                    //Console.WriteLine("\n\n" + e.Message);
                }
                Console.WriteLine(Check(ekeys) + "wrong key type");
                var eentities = false;
                try
                {
                    var mapMiss = new ElasticContext(
                        new ElasticConfiguration
                        {
                            Nodes = new List<string> { "http://development.elasticsearch-01.private.optio.ai:9200/" }
                        },
                        new Dictionary<string, Type>
                        {
                            { "datiko-test-longs", typeof(TestEntityLong) }
                        }
                    );
                    await mapMiss.Insert(new TestEntityString { });
                }
                catch (Exception e)
                {
                    eentities = true;
                    //Console.WriteLine("\n\n" + e.Message);
                }
                Console.WriteLine(Check(eentities) + "wrong entity");

                // ***insert same
                var einserts = false;
                try
                {
                    await elastic.Insert(new List<TestEntityLong> { entityLong777 });
                    await elastic.Insert(new List<TestEntityLong> { entityLong777, entityLong888 });
                }
                catch (Exception e)
                {
                    einserts = true;
                    //Console.WriteLine("\n\n" + e.Message + "\n\n");
                }
                Console.WriteLine(Check(einserts) + "duplicate insert\n");

                await elastic.Delete<TestEntityLong>();
                await elastic.Delete<TestEntityString>();

                // ***insert
                await elastic.Insert(entityString111);
                await elastic.Insert(entityLong777);
                entityString222.Delete = true;
                entityLong888.Delete = true;
                await elastic.Insert(new List<TestEntityString> { entityString222, entityString333 });
                await elastic.Insert(new List<TestEntityLong> { entityLong888, entityLong999 });
                entityString222.Delete = false;
                entityLong888.Delete = false;

                var entityStrings = await elastic.Load<TestEntityString>();
                var entityLongs = await elastic.Load<TestEntityLong>();

                Console.WriteLine(Check(entityStrings.Count == 3 && entityLongs.Count == 3) + " insert");

                // ***save
                entityString111.Name = "newname";
                entityLong777.Name = "newname";
                await elastic.Save(entityString111);
                await elastic.Save(entityLong777);
                entityString222.Name = "dt";
                entityString222.Date = new DateTime(1988, 11, 11);
                entityLong888.Name = "dt";
                entityLong888.Date = new DateTime(1988, 11, 11);
                await elastic.Save(new List<TestEntityString> { entityString111, entityString222, entityString333 });
                await elastic.Save(new List<TestEntityLong> { entityLong777, entityLong888, entityLong999 });

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();

                var check = new List<string>
                {
                    Check(entityStrings.GroupBy(i => i.Name).Count() == 3),
                    Check(entityLongs.GroupBy(i => i.Name).Count() == 3),
                    Check(entityStrings.GroupBy(i => i.Date).Count() == 2),
                    Check(entityLongs.GroupBy(i => i.Date).Count() == 2)
                };
                Console.WriteLine(Check(check.All(i => i == "=OK= ")) + " save full");

                await elastic.Save<TestEntityString>(333, new { Name = "dt", Date = new DateTime(1111, 1, 1) });
                await elastic.Save<TestEntityLong>(999, new { Name = "dt", Date = new DateTime(1111, 1, 1) });

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();

                check = new List<string>
                {
                    Check(entityStrings.GroupBy(i => i.Name).Count() == 2),
                    Check(entityLongs.GroupBy(i => i.Name).Count() == 2),
                    Check(entityStrings.GroupBy(i => i.Date).Count() == 3),
                    Check(entityLongs.GroupBy(i => i.Date).Count() == 3)
                };

                await elastic.Save<TestEntityString>(new Dictionary<object, object>
                {
                    { "111", new { Date = new DateTime(1988, 11, 11), Name = "datiko" } },
                    { "222", new { Name = "datiko" } },
                    { "333", new { Date = new DateTime(2222, 2, 2)  } }
                });
                await elastic.Save<TestEntityLong>(new Dictionary<object, object>
                {
                    { 777, new { Date = new DateTime(1988, 11, 11), Name = "datiko" } },
                    { 888, new { Name = "datiko" } },
                    { 999, new { Date = new DateTime(2222, 2, 2) } }
                });

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();

                check.Add(Check(entityStrings.GroupBy(i => i.Name).Count() == 2));
                check.Add(Check(entityLongs.GroupBy(i => i.Name).Count() == 2));
                check.Add(Check(entityStrings.GroupBy(i => i.Date).Count() == 2));
                check.Add(Check(entityLongs.GroupBy(i => i.Date).Count() == 2));
                Console.WriteLine(Check(check.All(i => i == "=OK= ")) + " save partial");

                await elastic.Save<TestEntityString>("000", new { Id = "000", Name = "000", Date = new DateTime(3333, 3, 3) }, true);
                await elastic.Save<TestEntityLong>(666, new { Id = 666, Name = "666", Date = new DateTime(3333, 3, 3) }, true);
                await elastic.Save<TestEntityString>(new Dictionary<object, object>
                {
                    { "000", new { Name = "=000=" } },
                    { "444", new { Id = "444", Name = "444", Date = new DateTime(4444, 4, 4) } }
                }, true);
                await elastic.Save<TestEntityLong>(new Dictionary<object, object>
                {
                    { 666, new { Name = "=666=" } },
                    { 555, new { Id = 555, Name = "555", Date = new DateTime(4444, 4, 4) } }
                }, true);

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();
                Console.WriteLine(Check(entityStrings.Count() == 5 && entityLongs.Count() == 5) + " save upsert");

                // ***read
                check = new List<string> { };

                var entityString = await elastic.Get<TestEntityString>("0");
                var entityLong = await elastic.Get<TestEntityLong>(0);
                check.Add(Check(entityString == null && entityLong == null));

                entityString = await elastic.Get<TestEntityString>("111");
                entityLong = await elastic.Get<TestEntityLong>(777);
                check.Add(Check(entityString != null && entityLong != null));

                entityString = await elastic.Get<TestEntityString>();
                entityLong = await elastic.Get<TestEntityLong>();
                check.Add(Check(entityString != null && entityLong != null));

                entityString = await elastic.Get<TestEntityString>(i => i.Sort(s => s.Descending("id.keyword")));
                entityLong = await elastic.Get<TestEntityLong>(i => i.Sort(s => s.Descending("id")));
                check.Add(Check(entityString != null && entityLong != null && entityString.Id == "444" && entityLong.Id == 999 && entityString.Date != null && entityLong.Date != null));

                entityString = await elastic.Get<TestEntityString>("111", f => f.Includes(i => i.Field("name")));
                entityLong = await elastic.Get<TestEntityLong>(999, f => f.Includes(i => i.Field("name")));
                check.Add(Check(entityString != null && entityLong != null && entityString.Name != null && entityLong.Name != null && entityString.Date == null && entityLong.Date == null));

                Console.WriteLine(Check(check.All(i => i == "=OK= ")) + " get");

                check = new List<string> { };

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();
                check.Add(Check(entityStrings.Count() == 5 && entityLongs.Count() == 5));

                entityStrings = await elastic.Load<TestEntityString>(new List<string> { });
                entityLongs = await elastic.Load<TestEntityLong>(new List<long> { });
                check.Add(Check(entityStrings.Count() == 0 && entityLongs.Count() == 0));

                entityStrings = await elastic.Load<TestEntityString>(new List<string> { "111" });
                entityLongs = await elastic.Load<TestEntityLong>(new List<long> { 777 });
                check.Add(Check(entityStrings.Count() == 1 && entityLongs.Count() == 1));

                entityStrings = await elastic.Load<TestEntityString>(new List<string> { "111", "222", "333" });
                entityLongs = await elastic.Load<TestEntityLong>(new List<long> { 777, 888, 999 });
                check.Add(Check(entityStrings.Count() == 3 && entityLongs.Count() == 3));

                entityStrings = await elastic.Load<TestEntityString>(new List<string> { "111" }, f => f.Includes(i => i.Field("name")));
                entityLongs = await elastic.Load<TestEntityLong>(new List<long> { 777 }, f => f.Includes(i => i.Field("name")));
                check.Add(Check(entityStrings.Count() == 1 && entityLongs.Count() == 1 && entityStrings[0].Date == null && entityLongs[0].Date == null));

                entityStrings = await elastic.Load<TestEntityString>(i => i.Sort(s => s.Descending("id.keyword")));
                entityLongs = await elastic.Load<TestEntityLong>(i => i.Sort(s => s.Descending("id")));
                check.Add(Check(entityStrings.Count() == 5 && entityLongs.Count() == 5 && entityStrings[0].Id == "444" && entityLongs[0].Id == 999));

                Console.WriteLine(Check(check.All(i => i == "=OK= ")) + " load");

                // ***delete
                check = new List<string> { };

                await elastic.Delete(new TestEntityString { Id = "000" });
                await elastic.Delete(new TestEntityLong { Id = 999 });
                await elastic.Delete<TestEntityString>("111");
                await elastic.Delete<TestEntityLong>(888);

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();
                check.Add(Check(entityStrings.Count() == 3 && entityLongs.Count() == 3));

                await elastic.Save(new List<TestEntityString> { new TestEntityString { Id = "222" }, new TestEntityString { Id = "333", Delete = true } });
                await elastic.Save(new List<TestEntityLong> { new TestEntityLong { Id = 777 }, new TestEntityLong { Id = 666, Delete = true } });

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();
                check.Add(Check(entityStrings.Count() == 2 && entityLongs.Count() == 2));

                await elastic.Delete<TestEntityLong>();
                await elastic.Delete<TestEntityString>();

                entityStrings = await elastic.Load<TestEntityString>();
                entityLongs = await elastic.Load<TestEntityLong>();
                check.Add(Check(entityStrings.Count() == 0 && entityLongs.Count() == 0));

                Console.WriteLine(Check(check.All(i => i == "=OK= ")) + " delete");

                Console.WriteLine("\n\nOOK Done");
                Console.ReadKey();

            }
            catch (Exception e)
            {

                throw;
            }


            Console.ReadKey();












        }
    }
}
