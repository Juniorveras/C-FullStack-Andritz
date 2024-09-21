using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Graph
{
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }

    public class Graph<T> : IGraph<T>
    {
        private readonly IEnumerable<ILink<T>> _links;
        public Graph(IEnumerable<ILink<T>> links)
        {
            _links = links;
        }
        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            return Observable.Create<IEnumerable<T>>(observer =>
            {
                var visited = new HashSet<T>();
                var routes = new List<IEnumerable<T>>();

                FindRoutes(source, target, visited, new List<T>(), routes);

                foreach (var route in routes)
                {
                    observer.OnNext(route);
                }

                observer.OnCompleted();

                return Disposable.Empty;
            });
        }

        private void FindRoutes(T currentNode, T target, HashSet<T> visited, List<T> currentRoute, List<IEnumerable<T>> routes)
        {
            visited.Add(currentNode);
            currentRoute.Add(currentNode);

            if (currentNode.Equals(target))
            {
                routes.Add(new List<T>(currentRoute));
            }
            else
            {
                foreach (var link in _links.Where(l => l.Source.Equals(currentNode) && !visited.Contains(l.Target)))
                {
                    FindRoutes(link.Target, target, visited, currentRoute, routes);
                }
            }

            currentRoute.RemoveAt(currentRoute.Count - 1);
            visited.Remove(currentNode);
        }
    }
}
