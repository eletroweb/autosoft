﻿using AutoSoft.Infrastructure.DependencyResolver;
using AutoSoft.Infrastructure.Domain;
using AutoSoft.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSoft.Infrastructure.Commands
{
    public class CommandSender : ICommandSender
    {
        private readonly IResolver _resolver;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEventStore _eventStore;

        public CommandSender(IResolver resolver,
            IEventPublisher eventPublisher,
            IEventStore eventStore)
        {
            _resolver = resolver;
            _eventPublisher = eventPublisher;
            _eventStore = eventStore;
        }

        public void Send<TCommand>(TCommand command, bool publishEvents = true) where TCommand : ICommand
        {
            var commandHandler = GetHandler<ICommandHandler<TCommand>, TCommand>(command);

            var events = commandHandler.Handle(command);

            if (!publishEvents)
                return;

            if (events != null)
            {
                foreach (var @event in events)
                {
                    var concreteEvent = EventFactory.CreateConcreteEvent(@event);
                    _eventPublisher.Publish(concreteEvent);
                }
            }
        }

        public void Send<TCommand, TAggregate>(TCommand command, bool publishEvents = true)
            where TCommand : ICommand
            where TAggregate : IAggregateRoot
        {
            var commandHandler = GetHandler<ICommandHandler<TCommand>, TCommand>(command);

            var events = commandHandler.Handle(command);

            if (events != null)
            {
                foreach (var @event in events)
                {
                    var concreteEvent = EventFactory.CreateConcreteEvent(@event);

                    _eventStore.SaveEvent<TAggregate>((IDomainEvent)concreteEvent);

                    if (!publishEvents)
                        continue;

                    _eventPublisher.Publish(concreteEvent);
                }
            }
        }

        public async Task SendAsync<TCommand>(TCommand command, bool publishEvents = true) where TCommand : ICommand
        {
            var commandHandler = GetHandler<ICommandHandlerAsync<TCommand>, TCommand>(command);

            var events = await commandHandler.HandleAsync(command);

            if (!publishEvents)
                return;

            foreach (var @event in events)
            {
                var concreteEvent = EventFactory.CreateConcreteEvent(@event);
                await _eventPublisher.PublishAsync(concreteEvent);
            }
        }

        public async Task SendAsync<TCommand, TAggregate>(TCommand command, bool publishEvents = true)
            where TCommand : ICommand
            where TAggregate : IAggregateRoot
        {
            var commandHandler = GetHandler<ICommandHandlerAsync<TCommand>, TCommand>(command);

            var events = await commandHandler.HandleAsync(command);

            foreach (var @event in events)
            {
                var concreteEvent = EventFactory.CreateConcreteEvent(@event);

                await _eventStore.SaveEventAsync<TAggregate>((IDomainEvent)concreteEvent);

                if (!publishEvents)
                    continue;

                await _eventPublisher.PublishAsync(concreteEvent);
            }
        }

        private THandler GetHandler<THandler, TCommand>(TCommand command) where TCommand : ICommand
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandHandler = _resolver.Resolve<THandler>();

            if (commandHandler == null)
                throw new Exception($"No handler found for command '{command.GetType().FullName}'");

            return commandHandler;
        }
    }
}
