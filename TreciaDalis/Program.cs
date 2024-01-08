
using System;
using System.Diagnostics;

class City
{
    public string Name { get; set; }
    public double GoodnessPoints { get; set; }

    public double XCord { get; set; } = 0;

    public double YCord { get; set; } = 0;
}

class RouteData
{
    public string CityFrom { get; set; }
    public string CityTo { get; set; }
    public int TimeInSeconds { get; set; }
    public double Cost { get; set; }
}


class Program
{
    static List<City> ReadCitiesFromCsv(string filePath)
    {
        List<City> cities = new List<City>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                string cityName = data[0];
                double goodnessPoints = Convert.ToDouble(data[1]);
                double xCord = Convert.ToDouble(data[2]);
                double yCord = Convert.ToDouble(data[3]);
                cities.Add(new City { Name = cityName, GoodnessPoints = goodnessPoints, XCord =  xCord, YCord = yCord});
            }
        }

        return cities;
    }

    static List<RouteData> ReadRoutesFromCsv(string filePath)
    {
        List<RouteData> routes = new List<RouteData>();

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');
                string cityFrom = data[0];
                string cityTo = data[1];
                int timeInSeconds = Convert.ToInt32(data[2]);
                double cost = Convert.ToDouble(data[3]);

                routes.Add(new RouteData { CityFrom = cityFrom, CityTo = cityTo, TimeInSeconds = timeInSeconds, Cost = cost });
            }
        }

        return routes;
    }



    static Random random = new Random();
    static List<City> cities = new List<City>();
    static List<RouteData> routes = new List<RouteData>();

    static int populationSize = 100;
    static int generations = 100;
    static double mutationRate = 0.3;
    static double crossRate = 0.9;
    static int timeLimit = 48 * 60 * 60;
    static string startingCity = "Varna";




    static void Main(string[] args)
    {
        // Load cities and route data
        cities = ReadCitiesFromCsv("cities.csv");
        routes = ReadRoutesFromCsv("routes.csv");
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        (List<City> solution, double points) = GeneticAlgorithm();
        sw.Stop();
        
        Console.WriteLine();
        PrintRoute(solution);
        Console.WriteLine("Goodness points: {0:0.000000000}", points);
        Console.WriteLine("Elapsed={0}", sw.Elapsed);

    }

    static (List<City>, double) GeneticAlgorithm()
    {
        List<List<City>> pop = new List<List<City>>();
        for (int i = 0; i < populationSize; i++)
        {
            List<City> randomSol = getRandomSolution();
            pop.Add(randomSol);
        }

        List<City> best = pop[0];
        double bestEval = Objective(pop[0]);


        for (int i = 0; i < generations; i++)
        {
            List<double> scores = new List<double>();
            for (int j = 0; j < pop.Count; j++)
            {
                scores.Add(Objective(pop[j]));
            }

            Console.WriteLine("Generation: {0}", i + 1);

            for (int j = 0; j < populationSize; j++)
            {
                if (scores[j] > bestEval)
                {
                    best = pop[j];
                    bestEval = scores[j];
                    Console.WriteLine("{0} , new best = {1:0.000000000}", i + 1, scores[j]);
                }
            }

            List<List<City>> parents = new List<List<City>>();
            for (int j = 0; j < populationSize; j++)
            {
                parents.Add(Selection(pop, scores));
            }
            // create the next generation
            List<List<City>> children = new List<List<City>>();
            for (int j = 0; j < populationSize; j += 2)
            {
                List<City> parent1 = parents[j];
                List<City> parent2 = parents[j + 1];
                (List<City> c1, List<City> c2) = Crossover(parent1, parent2);

                Mutation(c1);
                Mutation(c2);

                children.Add(c1);
                children.Add(c2);
            }
            pop = children;
        }
        return (best, bestEval);
    }

    static double Objective(List<City> randomSollution)
    {
        List<City> validSolution = makeValidSolution(randomSollution);
        randomSollution = validSolution;
        List<City> visitedAlready = new List<City>();
        double totalScore = 0;
        foreach (var city in validSolution)
        {
            if (!visitedAlready.Contains(city))
            {
                visitedAlready.Add(city);
                totalScore += city.GoodnessPoints;
            }
        }
        return totalScore;
    }

    static List<City> Selection(List<List<City>> pop, List<double> scores)
    {
        int selectionIndex = random.Next(0, pop.Count);
        int randIndex = random.Next(0, pop.Count);
        for (int i = 0; i < 2; i++)
        {
            if (scores[randIndex] > scores[selectionIndex])
            {
                selectionIndex = randIndex;
            }
            randIndex = random.Next(0, pop.Count);
        }
        return pop[selectionIndex];
    }

    static (List<City>, List<City>) Crossover(List<City> p1, List<City> p2)
    {
        List<City> c1 = new List<City>(p1);
        List<City> c2 = new List<City>(p2);
        
        if (random.NextDouble() < crossRate && p1.Count > 2 && p2.Count > 2)
        {
            int pt = random.Next(p1.Count / 2, p1.Count - 2);
            int pt2 = random.Next(p2.Count / 2, p2.Count - 2);
            // perform crossover
            List<City> temp = c1.GetRange(pt, c1.Count - pt);
            List<City> temp2 = c2.GetRange(pt2, c2.Count - pt2);
            c1.RemoveRange(pt, c1.Count - pt);
            c2.RemoveRange(pt2, c2.Count - pt2);
            c1.AddRange(temp2);
            c2.AddRange(temp);
        }
        return (c1, c2);
    }

    static void Mutation(List<City> sol)
    {
        for (int i = 1; i < sol.Count - 1; i++)
        {
            // check for a mutation
            if (random.NextDouble() < mutationRate)
            {
                int randCity = random.Next(cities.Count);
                sol[i] = cities[randCity];
            }
        }
    }

    static List<City> getRandomSolution()
    {
        List<City> randomSollution = new List<City>();
        randomSollution.Add(cities.Find(r => r.Name == startingCity));
        int randomIndex = 0;
        for (int i = 1; i < routes.Count; i++)
        {
            randomIndex = random.Next(0, cities.Count);
            while (randomSollution[i - 1] == cities[randomIndex] || (i == routes.Count - 1 && cities[randomIndex] == randomSollution[0]))
            {
                randomIndex = random.Next(0, cities.Count);
            }
            randomSollution.Add(cities[randomIndex]);
        }
        randomSollution.Add(randomSollution[0]);
        return randomSollution;
    }
    static List<City> makeValidSolution(List<City> initialSol)
    {
        int timeLeft = timeLimit;
        List<City> finalSolution = new List<City>();

        City startPoint = initialSol[0];
        finalSolution.Add(startPoint);
        bool moveForward = true;
        int nextCity = 1;
        while (moveForward)
        {
            if (timeLeft < 0 || initialSol.Count == nextCity)
            {        
                break;
            }
            RouteData routeData = routes.Find(r => r.CityFrom == finalSolution.Last().Name && r.CityTo == initialSol[nextCity].Name);
            
            if (routeData != null)
            {
                timeLeft -= routeData.TimeInSeconds;
                finalSolution.Add(initialSol[nextCity]);
            }

            nextCity++;
        }
        bool valid = false;
        City remCity = new City();
        while (!valid)
        {
            if (finalSolution.Count == 1)
            {
                break;
            }
            
            if (timeLeft < 0)
            {
                remCity = finalSolution.Last();
                finalSolution.RemoveAt(finalSolution.Count - 1);
                RouteData routeData = routes.Find(r => r.CityFrom == finalSolution.Last().Name && r.CityTo == remCity.Name);

                timeLeft += routeData.TimeInSeconds;
            }
            else
            {
                RouteData routeData = (routes.Find(r => r.CityFrom == finalSolution.Last().Name && r.CityTo == startPoint.Name));
                
                if (routeData == null || routeData.TimeInSeconds > timeLeft)
                {
                    remCity = finalSolution.Last();
                    finalSolution.RemoveAt(finalSolution.Count - 1);
                    timeLeft += routes.Find(r => r.CityFrom == finalSolution.Last().Name && r.CityTo == remCity.Name).TimeInSeconds;
                }
                else
                {
                    finalSolution.Add(startPoint);
                    valid = true;
                }
            }
        }
        return finalSolution;
    }

    static void PrintRoute(List<City> route)
    {
        route = makeValidSolution(route);
        int timeTaken = 0;
        Console.WriteLine("Best Route:");
        RouteData data = new RouteData();
        for (int i = 1; i < route.Count; i++)
        {
            if (i == 1)
            {
                Console.WriteLine(route[0].Name + " " + route[0].XCord + " " + route[0].YCord);
            }
            Console.WriteLine(route[i].Name + " " + route[i].XCord + " " + route[i].YCord);
            data = routes.Find(r => r.CityFrom == route[i - 1].Name && r.CityTo == route[i].Name);
            timeTaken += data.TimeInSeconds;
        }
        Console.WriteLine("Duration: {0:0.00} h", (double)timeTaken / 60 / 60);
    }
}
