using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
// Taken and modified from
// https://raw.githubusercontent.com/KayDeeTee/HK-NGG/master/src/

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace SkipCutscene
{
    public static class FsmUtil
    {
        public static void RemoveAction(this FsmState state, int index)
        {
            state.Actions = state.Actions.Where((_, ind) => ind != index).ToArray();
        }

        public static void RemoveAction<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.RemoveFirst(x => x is T).ToArray();
        }

        public static void RemoveAllOfType<T>(this FsmState state) where T : FsmStateAction
        {
            state.Actions = state.Actions.Where(x => x is not T).ToArray();
        }

        public static void RemoveAnim(this PlayMakerFSM fsm, string stateName, int index)
        {
            var anim = fsm.GetAction<Tk2dPlayAnimationWithEvents>(stateName, index);

            var @event = new FsmEvent(anim.animationCompleteEvent ?? anim.animationTriggerEvent);

            FsmState state = fsm.GetState(stateName);

            state.RemoveAction(index);

            state.InsertAction
            (
                index,
                new NextFrameEvent
                {
                    sendEvent = @event,
                    Active = true,
                    Enabled = true
                }
            );
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName)
        {
            return fsm.FsmStates.First(t => t.Name == stateName);
        }

        public static bool TryGetState(this PlayMakerFSM fsm, string stateName, out FsmState state)
        {
            state = fsm.FsmStates.FirstOrDefault(t => t.Name == stateName);

            return state != null;
        }

        public static FsmState CopyState(this PlayMakerFSM fsm, string stateName, string newState)
        {
            FsmState orig = fsm.GetState(stateName);

            var state = new FsmState(orig)
            {
                Name = newState,
                Transitions = orig
                              .Transitions
                              .Select(x => new FsmTransition(x) { ToFsmState = x.ToFsmState })
                              .ToArray(),
            };


            fsm.Fsm.States = fsm.FsmStates.Append(state).ToArray();

            return state;
        }

        public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction
        {
            FsmStateAction act = state.Actions[index];

            return (T)act;
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            FsmStateAction act = fsm.GetState(stateName).Actions[index];

            return (T)act;
        }

        public static T GetAction<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().First();
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName) where T : FsmStateAction
        {
            return fsm.GetState(stateName).GetAction<T>();
        }

        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            state.Actions = state.Actions.Append(action).ToArray();

            action.Init(state);
        }

        public static void InsertAction(this FsmState state, int index, FsmStateAction action)
        {
            state.Actions = state.Actions.Insert(index, action).ToArray();

            action.Init(state);
        }

        public static void ChangeTransition(this PlayMakerFSM self, string state, string eventName, string toState)
        {
            self.GetState(state).ChangeTransition(eventName, toState);
        }

        public static void ChangeTransition(this FsmState state, string eventName, string toState)
        {
            state.Transitions.First(tr => tr.EventName == eventName).ToFsmState = state.Fsm.FsmComponent.GetState(toState);
        }

        public static void AddTransition(this FsmState state, FsmEvent @event, string toState)
        {
            state.Transitions = state.Transitions.Append
            (
                new FsmTransition
                {
                    FsmEvent = @event,
                    ToFsmState = state.Fsm.GetState(toState)
                }
            )
            .ToArray();
        }

       
        public static void AddTransition(this FsmState state, string eventName, string toState)
        {
            state.AddTransition(FsmEvent.GetFsmEvent(eventName) ?? new FsmEvent(eventName), toState);
        }

       
        public static void RemoveTransition(this FsmState state, string transition)
        {
            state.Transitions = state.Transitions.Where(trans => transition != trans.ToFsmState.Name).ToArray();
        }

       
        public static void AddCoroutine(this FsmState state, Func<IEnumerator> method)
        {
            state.InsertCoroutine(state.Actions.Length, method);
        }

       
        public static void AddMethod(this FsmState state, Action method)
        {
            state.InsertMethod(state.Actions.Length, method);
        }

       
        public static void InsertMethod(this FsmState state, int index, Action method)
        {
            state.InsertAction(index, new InvokeMethod(method));
        }

       
        public static void InsertCoroutine(this FsmState state, int index, Func<IEnumerator> coro, bool wait = true)
        {
            state.InsertAction(index, new InvokeCoroutine(coro, wait));
        }

       
        public static FsmInt GetOrCreateInt(this PlayMakerFSM fsm, string intName)
        {
            FsmInt? prev = fsm.FsmVariables.IntVariables.FirstOrDefault(x => x.Name == intName);

            if (prev != null)
                return prev;

            var @new = new FsmInt(intName);

            fsm.FsmVariables.IntVariables = fsm.FsmVariables.IntVariables.Append(@new).ToArray();

            return @new;
        }

       
        public static FsmBool CreateBool(this PlayMakerFSM fsm, string boolName)
        {
            var @new = new FsmBool(boolName);

            fsm.FsmVariables.BoolVariables = fsm.FsmVariables.BoolVariables.Append(@new).ToArray();

            return @new;
        }

       
        public static void AddToSendRandomEventV3
        (
            this SendRandomEventV3 sre,
            string toState,
            float weight,
            int eventMaxAmount,
            int missedMaxAmount,
            string? eventName = null,
            bool createInt = true
        )
        {
            var fsm = (PlayMakerFSM)sre.Fsm.Owner;

            string state = sre.State.Name;

            eventName ??= toState.Split(' ').First();

            fsm.GetState(state).AddTransition(eventName, toState);

            sre.events = sre.events.Append(fsm.GetState(state).Transitions.Single(x => x.FsmEvent.Name == eventName).FsmEvent).ToArray();
            sre.weights = sre.weights.Append(weight).ToArray();
            sre.trackingInts = sre.trackingInts.Append(fsm.GetOrCreateInt($"Ms {eventName}")).ToArray();
            sre.eventMax = sre.eventMax.Append(eventMaxAmount).ToArray();
            sre.trackingIntsMissed = sre.trackingIntsMissed.Append(fsm.GetOrCreateInt($"Ct {eventName}")).ToArray();
            sre.missedMax = sre.missedMax.Append(missedMaxAmount).ToArray();
        }

        public static FsmState CreateState(this PlayMakerFSM fsm, string stateName)
        {
            var state = new FsmState(fsm.Fsm)
            {
                Name = stateName
            };

            fsm.Fsm.States = fsm.FsmStates.Append(state).ToArray();

            return state;
        }
    }
}


///////////////////////
// Method Invocation //
///////////////////////

public class InvokeMethod : FsmStateAction
    {
        private readonly Action _action;

        public InvokeMethod(Action a)
        {
            _action = a;
        }

        public override void OnEnter()
        {
            _action?.Invoke();
            Finish();
        }
    }

public class InvokeCoroutine : FsmStateAction
{
    private readonly Func<IEnumerator> _coro;
    private readonly bool _wait;

    public InvokeCoroutine(Func<IEnumerator> f, bool wait)
    {
        _coro = f;
        _wait = wait;
    }

    private IEnumerator Coroutine()
    {
        yield return _coro.Invoke();
        Finish();
    }

    public override void OnEnter()
    {
        Fsm.Owner.StartCoroutine(_wait ? Coroutine() : _coro.Invoke());

        if (!_wait) Finish();
    }
}


    ////////////////
    // Extensions //
    ////////////////


