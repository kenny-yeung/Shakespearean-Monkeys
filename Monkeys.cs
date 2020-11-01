namespace Monkeys {
    using Carter;
    using Carter.ModelBinding;
    using Carter.Request;
    using Carter.Response;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using static System.Console;
    
    public class HomeModule : CarterModule {
        public HomeModule () {
            Post ("/try", async (req, res) => {
                // ... UDOO
                var a1 = await req.Bind<TryRequest> ();
                //Dynamic Length if(a1.length == 0){
                //}
                GeneticAlgorithm(a1);
                await Task.Delay (0);
                return;
            });
        }
        
        async Task<AssessResponse> PostFitnessAssess (AssessRequest areq) {
            // ... UDOO
            await Task.Delay (0);
            var client = new HttpClient ();
            client.BaseAddress = new Uri ("http://localhost:8091/");
            client.DefaultRequestHeaders.Accept.Clear ();
            client.DefaultRequestHeaders.Accept.Add (
                new MediaTypeWithQualityHeaderValue ("application/json"));
                //post the same text to asses on Fitness
            var hrm = await client.PostAsJsonAsync ("/assess", areq);
            hrm.EnsureSuccessStatusCode ();

            var ares = await hrm.Content.ReadAsAsync<AssessResponse>();
        
            return ares;
        }
        
        async Task PostClientTop (TopRequest treq) {
            // ... UDOO
            await Task.Delay (0);

            var client = new HttpClient ();
            client.BaseAddress = new Uri ("http://localhost:"+treq.id.ToString()+"/");
            client.DefaultRequestHeaders.Accept.Clear ();
            client.DefaultRequestHeaders.Accept.Add (
                new MediaTypeWithQualityHeaderValue ("application/json"));
            //post the same text to asses on Fitness
            var hrm = await client.PostAsJsonAsync ("/top", treq);
            hrm.EnsureSuccessStatusCode ();

            return;
        }
        
        private Random _random = new Random (1);
        
        private double NextDouble () {
            lock (this) {
                return _random.NextDouble ();
            }
        }
        
        private int NextInt (int a, int b) {
            lock (this) {
                return _random.Next (a, b);
            }
        }

        int ProportionalRandom (int[] weights, int sum) {
            var val = NextDouble () * sum;
            
            for (var i = 0; i < weights.Length; i ++) {
                if (val < weights[i]) return i;
                
                val -= weights[i];
            }
            
            WriteLine ($"***** Unexpected ProportionalRandom Error");
            return 0;
        }

        async void GeneticAlgorithm (TryRequest treq) {
            WriteLine ($"..... GeneticAlgorithm {treq}");
            await Task.Delay (0);
            
            // just an ad-hoc PR test - you will remove this
            // await ProportionalRandomTest ();
            
            // YOU CODE GOES HERE
            // FOLLOW THE GIVEN PSEUDOCODE
            
            // ...
            var id = treq.id; //This is the port number
            var monkeys = treq.monkeys; // number of monkeys
            if (monkeys % 2 != 0) monkeys += 1;
            var length = treq.length; 
            var crossover = treq.crossover / 100.0 ;
            var mutation = treq.mutation / 100.0;
            var limit = treq.limit;
            if (limit == 0) limit = 1000;
            var topscore = int.MaxValue;

            // ... UDOO
            List<string> genomes = new List<string>();
            for(int i = 0; i< monkeys; i++){
                string s = "";
                for(int j = 0; j < length; j++){
                    char genChar = (char)NextInt(32, 127);
                    s += genChar.ToString();
                }
                genomes.Add(s);
            }
            
            var obj1 = new AssessRequest{id = treq.id, genomes = genomes};

            for (int loop = 0; loop < limit; loop ++) {
                // ...
                var x = await PostFitnessAssess(obj1);
                var response_id = x.id;
                var response_scores = x.scores;
                if(topscore > response_scores.Min()){
                    topscore = response_scores.Min();
                    int index = response_scores.IndexOf(response_scores.Min());
                    var genome = obj1.genomes.ElementAt(index);
                    var obj2 = new TopRequest{id = treq.id, loop = loop, score = topscore, genome = genome};
                    var client = PostClientTop(obj2);
                }
                if(topscore == 0){
                    break;
                }

                var weights = response_scores.Select(n => response_scores.Max() - n + 1);
                var sumofweights = weights.Sum();

                var para = treq.parallel;
                var new_genomes = obj1.genomes;
                if(para == true){//parallel section
                    new_genomes = ParallelEnumerable.Range(1, monkeys/2).SelectMany<int, string>(i => {
                    var index1 = ProportionalRandom(weights.ToArray(), sumofweights);
                    var index2 = ProportionalRandom(weights.ToArray(), sumofweights);

                    var p1 = obj1.genomes[index1];
                    var p2 = obj1.genomes[index2];
                    var c1 = p1;
                    var c2 = p2;
                    if(NextDouble()< crossover){
                        var crossIndex = NextInt(0, c1.Length);
                        c1 = p1.Substring(0, crossIndex) + p2.Substring(crossIndex);
                        c2 = p2.Substring(0, crossIndex) + p1.Substring(crossIndex);
                    }

                    if(NextDouble() < mutation){
                        var randIndex1 = NextInt(0, c1.Length);
                        char randChar1 = (char)NextInt(32, 127);
                        char[] array1 = c1.ToCharArray();
                        array1[randIndex1] = randChar1;
                        c1 = new string(array1);
                    }

                    if(NextDouble() < mutation){
                        var randIndex2 = NextInt(0, c2.Length);
                        char randChar2 = (char)NextInt(32, 127);
                        char[] array2 = c2.ToCharArray();
                        array2[randIndex2] = randChar2;
                        c2 = new string(array2);
                    }
                    return new[]{c1, c2};
                }).ToList();
                }
                else{   //non-parallel section
                        new_genomes = Enumerable.Range(1, monkeys/2).SelectMany<int, string>(i => {
                        var index1 = ProportionalRandom(weights.ToArray(), sumofweights);
                        var index2 = ProportionalRandom(weights.ToArray(), sumofweights);

                        var p1 = obj1.genomes[index1];
                        var p2 = obj1.genomes[index2];
                        var c1 = p1;
                        var c2 = p2;
                        if(NextDouble()< crossover){
                            var crossIndex = NextInt(0, c1.Length);
                            c1 = p1.Substring(0, crossIndex) + p2.Substring(crossIndex);
                            c2 = p2.Substring(0, crossIndex) + p1.Substring(crossIndex);
                        }

                        if(NextDouble() < mutation){
                            var randIndex1 = NextInt(0, c1.Length);
                            char randChar1 = (char)NextInt(32, 127);
                            char[] array1 = c1.ToCharArray();
                            array1[randIndex1] = randChar1;
                            c1 = new string(array1);
                        }

                        if(NextDouble() < mutation){
                            var randIndex2 = NextInt(0, c2.Length);
                            char randChar2 = (char)NextInt(32, 127);
                            char[] array2 = c2.ToCharArray();
                            array2[randIndex2] = randChar2;
                            c2 = new string(array2);
                        }
                        return new[]{c1, c2};
                    }).ToList();
                }

                obj1.genomes = new_genomes;
                obj1.id = response_id;

            }
        }
    }
    
    //public class TargetRequest {
    //     public int id { get; set; }
    //     public bool parallel { get; set; }
    //     public string target { get; set; }
    //     public override string ToString () {
    //         return $"{{{id}, {parallel}, \"{target}\"}}";
    //     }  
    // }    

    public class TryRequest {//client to monkeys
        public int id { get; set; }
        public bool parallel { get; set; }
        public int monkeys { get; set; }
        public int length { get; set; }
        public int crossover { get; set; }
        public int mutation { get; set; }
        public int limit { get; set; }
        public override string ToString () {
            return $"{{{id}, {parallel}, {monkeys}, {length}, {crossover}, {mutation}, {limit}}}";
        }
    }
    
    public class TopRequest { //monkeys to client
        public int id { get; set; }
        public int loop { get; set; }
        public int score { get; set; }
        public string genome { get; set; }
        public override string ToString () {
            return $"{{{id}, {loop}, {score}, {genome}}}";
        }  
    }    
    
    public class AssessRequest { //monkeys to fitness
        public int id { get; set; }
        public List<string> genomes { get; set; }
        public override string ToString () {
            return $"{{{id}, #{genomes.Count}}}";
        }  
    }
    
    public class AssessResponse {// fitness to monkeys
        public int id { get; set; }
        public List<int> scores { get; set; }
        public override string ToString () {
            return $"{{{id}, #{scores.Count}}}";
        }  
    }   
}

namespace Monkeys {
    using Carter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup {
        public void ConfigureServices (IServiceCollection services) {
            services.AddCarter ();
        }

        public void Configure (IApplicationBuilder app) {
            app.UseRouting ();
            app.UseEndpoints( builder => builder.MapCarter ());
        }
    }
}

namespace Monkeys {
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Program {
        public static void Main (string[] args) {
//          var host = Host.CreateDefaultBuilder (args)
//              .ConfigureWebHostDefaults (webBuilder => webBuilder.UseStartup<Startup>())

            var urls = new[] {"http://localhost:8081"};
            
            var host = Host.CreateDefaultBuilder (args)
            
                .ConfigureLogging (logging => {
                    logging
                        .ClearProviders ()
                        .AddConsole ()
                        .AddFilter (level => level >= LogLevel.Warning);
                })
                
                .ConfigureWebHostDefaults (webBuilder => {
                    webBuilder.UseStartup<Startup> ();
                    webBuilder.UseUrls (urls);  // !!!
                })
                
                .Build ();
            
            System.Console.WriteLine ($"..... starting on {string.Join (", ", urls)}");            
            host.Run ();
        }
    }
}

