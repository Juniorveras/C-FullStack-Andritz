using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Graph
{
    //A interface IGraph<T> define um contrato para uma estrutura de grafo genérica onde os nós são do tipo T.
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }
    //A classe Graph<T> implementa a interface IGraph<T>.
    //O construtor da classe aceita uma coleção de links(_links),
    //que representam as conexões entre os nós do Gragh.
    //Cada link deve implementar a interface ILink<T>, que define os nós de origem e destino.
    public class Graph<T> : IGraph<T>
    {
        private readonly IEnumerable<ILink<T>> _links;
        public Graph(IEnumerable<ILink<T>> links)
        {
            _links = links;
        }
        //Este método é a implementação do método da interface IGraph<T>. Ele cria um observable que procura todas as rotas possíveis entre os nós source(origem) e target(destino).
        //O método usa Reactive Extensions(Rx) para emitir essas rotas.        
        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            return Observable.Create<IEnumerable<T>>(observer =>
            {
                //Cria um HashSet<T> para armazenar os nós já visitados, evitando ciclos.
                var visited = new HashSet<T>();
                //Cria uma lista routes para armazenar todas as rotas encontradas.
                var routes = new List<IEnumerable<T>>();
                //O método auxiliar FindRoutes é chamado para procurar as rotas a partir de um nó inicial.
                FindRoutes(source, target, visited, new List<T>(), routes);

                foreach (var route in routes)
                {
                    //As rotas encontradas são emitidas para o fluxo reativo.
                    observer.OnNext(route);
                }
                //Quando todas as rotas são processadas, o observable é completado.
                observer.OnCompleted();
                //Retorna Disposable.Empty porque não é necessário liberar recursos específicos no final.
                return Disposable.Empty;
            });
        }
        //Este é um método recursivo responsável por explorar todas as rotas do nó atual (currentNode) até o nó de destino (target).        
        private void FindRoutes(T currentNode, T target, HashSet<T> visited, List<T> currentRoute, List<IEnumerable<T>> routes)
        {
            //Marca o nó atual como visitado, adicionando-o ao HashSet<T> visited para evitar voltar a ele no mesmo caminho.
            visited.Add(currentNode);
            //Adiciona o nó atual à lista currentRoute, que mantém a rota que está a ser explorada.
            currentRoute.Add(currentNode);
            
            //Se o nó atual for igual ao nó de destino (target), copia a rota atual para a lista de rotas finais (routes).
            if (currentNode.Equals(target))
            {
                routes.Add(new List<T>(currentRoute));
            }
            else
            {
                //Caso contrário, verifica todos os links que partem do nó atual e que levam a nós ainda não visitados.
                //Para cada um desses nós, chama-se recursivamente o método FindRoutes.
                foreach (var link in _links.Where(l => l.Source.Equals(currentNode) && !visited.Contains(l.Target)))
                {
                    FindRoutes(link.Target, target, visited, currentRoute, routes);
                }
            }
            //Depois de explorar todas as possíveis rotas a partir do nó atual,
            //o método desfaz a última adição à rota e remove o nó atual da lista de nós visitados,
            //permitindo que o algoritmo explore outras rotas que possam passar por esse nó mais tarde.
            currentRoute.RemoveAt(currentRoute.Count - 1);
            visited.Remove(currentNode);
        }
    }
}