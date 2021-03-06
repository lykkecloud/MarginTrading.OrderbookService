﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.MarginTrading.BrokerBase;
using Lykke.MarginTrading.BrokerBase.Models;
using Lykke.MarginTrading.BrokerBase.Settings;
using Lykke.MarginTrading.OrderBookService.Contracts.Models;
using Lykke.SlackNotifications;
using MarginTrading.OrderBookService.Core.Repositories;

namespace MarginTrading.OrderBookService.ExecutionOrderBookBroker
{
    public class Application : BrokerApplicationBase<OrderExecutionOrderBookContract>
    {
        private readonly IExecutionOrderBookRepository _executionOrderBookRepository;
        private readonly ILog _log;
        private readonly Settings _settings;

        public Application(
            IExecutionOrderBookRepository executionOrderBookRepository,
            ILog logger,
            Settings settings, 
            CurrentApplicationInfo applicationInfo,
            ISlackNotificationsSender slackNotificationsSender) 
        : base(logger, slackNotificationsSender, applicationInfo, MessageFormat.MessagePack)
        {
            _executionOrderBookRepository = executionOrderBookRepository;
            _log = logger;
            _settings = settings;
        }

        protected override BrokerSettingsBase Settings => _settings;
        protected override string ExchangeName => _settings.RabbitMqQueues.ExecutionOrderBooks.ExchangeName;
        public override string RoutingKey => "OrderExecutionOrderBookContract";
        
        protected override Task HandleMessage(OrderExecutionOrderBookContract orderBookMessage)
        {
            var orderBook = orderBookMessage.ToDomain();
            
            return Task.Run(async () =>
            {
                try
                {
                    await _executionOrderBookRepository.AddAsync(orderBook);
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(ExecutionOrderBookBroker), nameof(HandleMessage), 
                        orderBook.ToJson(), ex);
                }
            });
        }
    }
}






















