using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraDES;

namespace Teste2
{
    class Program
    {
        static void Main(string[] args)
        {
            // CONSTANTS
            int nShips = 4;                                                                 // Definição da Complexidade do Problema
            int nVerticalConveyorBelt = 2 * nShips - 1;                                       // Quantidade de Correias Transportadoras Verticais         
            int nHorizontalConveyorBelt = nShips - 1;                                       // Quantidade de Correias Transportadoras Horizontais
            int nStates = nVerticalConveyorBelt * 2 + nHorizontalConveyorBelt * 3
                + nHorizontalConveyorBelt * 2 + (nVerticalConveyorBelt - 1) * 2;                  // Número de Estados Plausíveis        
            int nTransitions = nVerticalConveyorBelt * 2 + nHorizontalConveyorBelt * 4;     // Número de Transições Plausíveis

            // PRESENTATION
            Console.WriteLine("Complexidade (número de navios): {0} ", nShips);
            Console.WriteLine("Correias Transportadoras Verticais: {0} ", nVerticalConveyorBelt);
            Console.WriteLine("Correias Transportadoras Horizontais: {0} ", nHorizontalConveyorBelt);
            Console.WriteLine("Estados Necessários (plantas e especificações): {0} ", nStates);
            Console.WriteLine("Transições Necessárias (plantas e especificações): {0} ", nTransitions);

            // Tempo para leitura das informações iniciais.
            System.Threading.Thread.Sleep(5000);

            // CREATING STATES (0 to nStates)
            var s =
                Enumerable.Range(0, nStates)
                    .Select(i =>
                            new State(i.ToString(),
                                i == 0
                                    ? Marking.Marked
                                    : Marking.Unmarked)
                    ).ToArray();

            // CREATING EVENTS (0 to nTransitions)
            var e =
                Enumerable.Range(0, nTransitions)
                    .Select(i =>
                        new Event(i.ToString(),
                            Controllability.Controllable

                     )
                    ).ToArray();

            // EVENT INDEXES MARKERS
            int horizontalLeftOnEvent = -1;
            int verticalOnEvent = -1;

            // COUNTERS
            int eventCounter = 0;                                                      // Event Counter
            int stateCounter = 0;                                                      // State Counter

            //---------------------------
            // Plants
            //----------------------------


            List<DeterministicFiniteAutomaton> conveyorBeltAutomatonList = new List<DeterministicFiniteAutomaton>();

            // Adding Esteira Vertical Automaton to a collection

            for (int i = 0; i < nVerticalConveyorBelt; i++)
            {
                conveyorBeltAutomatonList.Add(new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[stateCounter], e[eventCounter], s[stateCounter+1]),        // Event: Vertical On
                    new Transition(s[stateCounter+1], e[eventCounter+1], s[stateCounter])       // Event: Vertical Off
                },
                s[stateCounter], "C" + i.ToString() + "V"));

                verticalOnEvent = verticalOnEvent < 0 ? eventCounter : verticalOnEvent;         // Gravando a primeira ocorrência de um evento de ligamento vertical

                eventCounter = eventCounter + 2;
                stateCounter = stateCounter + 2;

            }

            // Appending Esteira Horizontal Automaton to the same collection

            for (int i = nVerticalConveyorBelt; i < nVerticalConveyorBelt + nHorizontalConveyorBelt; i++)
            {
                conveyorBeltAutomatonList.Add(new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[stateCounter], e[eventCounter], s[stateCounter+1]),            // Event: Horizontal Left On
                    new Transition(s[stateCounter+1], e[eventCounter+1], s[stateCounter]),          // Event: Horizontal Left Off
                    new Transition(s[stateCounter], e[eventCounter+2], s[stateCounter+2]),          // Event: Horizontal Right On
                    new Transition(s[stateCounter+2], e[eventCounter+3], s[stateCounter])           // Event: Horizontal Right Off
                },
                s[stateCounter], "C" + i.ToString() + "H"));

                horizontalLeftOnEvent = horizontalLeftOnEvent < 0 ? eventCounter : horizontalLeftOnEvent;       // Gravando a primeira ocorrência de 
                                                                                                                // ligamento horizontal

                eventCounter = eventCounter + 4;
                stateCounter = stateCounter + 3;
            }

            int horizontalRightOnEvent = horizontalLeftOnEvent + 2;
            int horizontalLeftOffEvent = horizontalLeftOnEvent + 1;

            //----------------------------
            // Specifications
            //----------------------------

            List<DeterministicFiniteAutomaton> specsList = new List<DeterministicFiniteAutomaton>();

            // HORIZONTAL CONVEYOR BELT SPEC

            int specIndex = 1;
            int verticalOffEvent = 1;
            int limit = horizontalLeftOnEvent + (nHorizontalConveyorBelt * 4);

            for (int i = horizontalLeftOnEvent; i < limit; i = i + 4)

            {
                specsList.Add(new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[stateCounter], e[verticalOffEvent], s[stateCounter+1]),
                    new Transition(s[stateCounter+1], e[horizontalLeftOnEvent], s[stateCounter]),
                    new Transition(s[stateCounter+1], e[horizontalRightOnEvent], s[stateCounter])
                },
                s[stateCounter], "E" + specIndex.ToString() + "H"));

                verticalOffEvent = verticalOffEvent + 2;
                horizontalLeftOnEvent = horizontalLeftOnEvent + 4;
                horizontalRightOnEvent = horizontalRightOnEvent + 4;

                stateCounter = stateCounter + 2;

                specIndex++;
            }



            // VERTICAL CONVEYOR BELT SPEC

            verticalOnEvent = 3;
            limit = horizontalLeftOffEvent + (nVerticalConveyorBelt - 1) / 2 * 4;

            for (int i = horizontalLeftOffEvent; i < limit; i = i + 4)
            {
                // Adicionando especificação para esteira alimentada pela direita
                specsList.Add(new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[stateCounter], e[horizontalLeftOffEvent], s[stateCounter+1]),
                    new Transition(s[stateCounter+1], e[verticalOnEvent], s[stateCounter])
                },
                s[stateCounter], "E" + specIndex.ToString() + "V"));

                // Adicionando especificação para esteira alimentada pela esquerda
                specsList.Add(new DeterministicFiniteAutomaton(
                new[]
                {
                    new Transition(s[stateCounter+2], e[horizontalLeftOffEvent+2], s[stateCounter+3]),
                    new Transition(s[stateCounter+3], e[verticalOnEvent+2], s[stateCounter+2])
                },
                s[stateCounter + 2], "E" + specIndex.ToString() + "V"));

                verticalOnEvent = verticalOnEvent + 4;
                horizontalLeftOnEvent = horizontalLeftOnEvent + 4;
                horizontalRightOnEvent = horizontalRightOnEvent + 4;

                stateCounter = stateCounter + 4;

                specIndex++;
            }




            //----------------------------------------
            // Supervisory Control 
            //----------------------------------------


            // COMPUTING THE MONOLITIC SUPERVISOR
            var timer = new Stopwatch();
            timer.Start();
            var sup = DeterministicFiniteAutomaton.MonoliticSupervisor(
                conveyorBeltAutomatonList.ToArray(), // Plants
                specsList.ToArray(), true);
            timer.Stop();

            Console.WriteLine("Computation Time: {0}", timer.ElapsedMilliseconds / 1000.0);

            // EXPORTING TO TCT
            sup.ToXMLFile("S.xml");

            Console.ReadLine();
        }
    }
}
