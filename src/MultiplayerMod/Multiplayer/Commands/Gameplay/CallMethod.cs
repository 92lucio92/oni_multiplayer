﻿using System;
using System.Linq;
using System.Reflection;
using MultiplayerMod.Game.Mechanics.Objects;
using MultiplayerMod.Multiplayer.Objects.Reference;

namespace MultiplayerMod.Multiplayer.Commands.Gameplay;

[Serializable]
public class CallMethod : MultiplayerCommand {
    private readonly ComponentReference? componentTarget;
    private readonly StateMachineReference? stateMachineTarget;
    private readonly Type methodType;
    private readonly string methodName;
    private readonly Type[]? parameters;
    private readonly object[] args;

    private const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                       BindingFlags.DeclaredOnly;

    public CallMethod(ComponentEventsArgs eventArgs) {
        componentTarget = eventArgs.Target;
        methodType = eventArgs.MethodType;
        methodName = eventArgs.MethodName;
        parameters = eventArgs.Parameters;
        args = eventArgs.Args;
    }

    public CallMethod(StateMachineEventsArgs eventArgs) {
        stateMachineTarget = eventArgs.Target;
        methodType = eventArgs.MethodType;
        methodName = eventArgs.MethodName;
        args = eventArgs.Args;
    }

    public override void Execute(MultiplayerCommandContext context) {
        var method = parameters == null
            ? methodType.GetMethod(methodName, flags)
            : methodType.GetMethod(methodName, flags, null, parameters, null);
        var args = this.args.Select(
            arg =>
                arg switch {
                    GameObjectReference gameObjectReference => gameObjectReference.GetGameObject(),
                    ComponentReference reference => reference.GetComponent(),
                    _ => arg
                }
        ).ToArray();
        object? obj = componentTarget != null ? componentTarget.GetComponent() : stateMachineTarget!.GetInstance();
        if (obj != null) {
            method?.Invoke(obj, args);
        }
    }

    public override string ToString() => $"{base.ToString()} (Type = {methodType.FullName}, Method = {methodName})";

}
