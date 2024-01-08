using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class City
{
    public string Name { get; set; }
    public double GoodnessPoints { get; set; }
}

public class Route
{
    public string CityFrom { get; set; }
    public string CityTo { get; set; }
    public int TimeInSeconds { get; set; }
    public double Cost { get; set; }
}

public class Program
{
    private const int TimeLimitInSeconds = 48 * 60 * 60;

    private static List<City> LoadCitiesFromCSV(string filePath)
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

                cities.Add(new City { Name = cityName, GoodnessPoints = goodnessPoints });
            }
        }

        return cities;
    }

    private static List<Route> LoadRoutesFromCSV(string filePath)
    {
        List<Route> routes = new List<Route>();

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

                routes.Add(new Route { CityFrom = cityFrom, CityTo = cityTo, TimeInSeconds = timeInSeconds, Cost = cost });
            }
        }

        return routes;
    }

    public static void Main()
    {
        List<City> cities = LoadCitiesFromCSV("cities2.csv");
        List<Route> routes = LoadRoutesFromCSV("routes2.csv");

        Console.WriteLine("Available cities:");
        foreach (var city in cities)
        {
            Console.WriteLine("- " + city.Name);
        }

        Console.WriteLine();
        Console.Write("Enter the starting city: ");
        string startingCity = Console.ReadLine();
        Stopwatch sw = new Stopwatch();
        Console.WriteLine();
        sw.Start();
        List<List<string>> allRoutes = GenerateAllRoutes(cities.Select(c => c.Name).ToList(), startingCity, routes);

        
        double maxGoodnessPoints = 0;
        List<string> bestRoute = null;

        foreach (var route in allRoutes)
        {
            double goodnessPoints = CalculateGoodnessPoints(route, cities);

            if (goodnessPoints > maxGoodnessPoints)
            {
                maxGoodnessPoints = goodnessPoints;
                bestRoute = route;
            }
        }
        sw.Stop();
        if (bestRoute == null)
        {
            Console.WriteLine("Nėra kelio");
            return;
        }
        Console.WriteLine("Best Route: " + string.Join(" -> ", bestRoute));
        Console.WriteLine();
        Console.WriteLine("Goodness Points: {0:0.000000000}", maxGoodnessPoints);
        int timeSpent = CalculateAccumulatedTime(bestRoute, routes);
        Console.WriteLine("Time spent: {0:0.00} h", (double)timeSpent / 60 / 60);
        Console.WriteLine("Elapsed={0}", sw.Elapsed);
    }


    private static List<List<string>> GenerateAllRoutes(List<string> cities, string startingCity, List<Route> routes)
    {
        List<List<string>> allRoutes = new List<List<string>>();
        Stack<List<string>> stack = new Stack<List<string>>();
        stack.Push(new List<string> { startingCity });

        while (stack.Count > 0)
        {
            List<string> currentRoute = stack.Pop();
            string lastCity = currentRoute.Last();
            int accumulatedTime = CalculateAccumulatedTime(currentRoute, routes);
            
            if (cities.Count == currentRoute.Distinct().Count() && currentRoute[0] == lastCity && accumulatedTime <= TimeLimitInSeconds)
            {
                allRoutes = new List<List<string>>();
                allRoutes.Add(currentRoute);
                break;
            }
            if (currentRoute.Count > 1 && currentRoute[0] == lastCity && accumulatedTime <= TimeLimitInSeconds)
            {
                allRoutes.Add(currentRoute);
            }
            foreach (var route in routes)
            {
                if (route.CityFrom == lastCity)
                {
                    if(lastCity == startingCity && currentRoute.Contains(route.CityTo))
                    {
                        continue;
                    }
                    if (route.CityTo == startingCity || !currentRoute.Contains(route.CityTo) || !ifRouteVisited(currentRoute, lastCity, route.CityTo))
                    {
                        List<string> newRoute = new List<string>(currentRoute);
                        newRoute.Add(route.CityTo);
                        stack.Push(newRoute);
                    }
                }
            }
        }

        return allRoutes;
    }

    private static bool ifRouteVisited(List<string> routes, string cityFrom, string cityTo)
    {
        for (int i = 0; i < routes.Count; i++)
        {
            if(i + 1 == routes.Count)
            {
                break;
            }
            if (routes[i]==cityFrom && routes[i+1]==cityTo)
            {
                return true;
            }
        }
        return false;
    }

    private static int CalculateAccumulatedTime(List<string> route, List<Route> routes)
    {
        int accumulatedTime = 0;

        for (int i = 0; i < route.Count - 1; i++)
        {
            string cityFrom = route[i];
            string cityTo = route[i + 1];

            Route matchingRoute = routes.FirstOrDefault(r =>r.CityFrom == cityFrom && r.CityTo == cityTo);

            if (matchingRoute != null)
            {
                accumulatedTime += matchingRoute.TimeInSeconds;
            }
        }
        return accumulatedTime;
    }

    private static double CalculateGoodnessPoints(List<string> route, List<City> cities)
    {
        double goodnessPoints = 0;
        List<string> visitedCities = new List<string>();

        foreach (var city in route)
        {
            City matchingCity = cities.FirstOrDefault(c => c.Name == city);

            if (matchingCity != null && !visitedCities.Contains(city))
            {
                goodnessPoints += matchingCity.GoodnessPoints;
                visitedCities.Add(city);
            }
        }

        return goodnessPoints;
    }
}

