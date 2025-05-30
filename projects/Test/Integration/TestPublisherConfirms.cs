﻿// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 2.0.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (c) 2007-2025 Broadcom. All Rights Reserved.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       https://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v2.0:
//
//---------------------------------------------------------------------------
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
//  Copyright (c) 2007-2025 Broadcom. All Rights Reserved.
//---------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Xunit;
using Xunit.Abstractions;

namespace Test.Integration
{
    public class TestPublisherConfirms : IntegrationFixture
    {
        private readonly byte[] _messageBody;

        public TestPublisherConfirms(ITestOutputHelper output)
            : base(output, openChannel: false)
        {
            _messageBody = GetRandomBody(4096);
        }

        [Fact]
        public async Task TestWaitForConfirmsWithEventsAsync()
        {
            string queueName = GenerateQueueName();
            await using IChannel ch = await _conn.CreateChannelAsync(_createChannelOptions);
            await ch.QueueDeclareAsync(queue: queueName, passive: false, durable: false,
                exclusive: true, autoDelete: false, arguments: null);

            int n = 200;
            // number of event handler invocations
            int c = 0;

            ch.BasicAcksAsync += (_, args) =>
            {
                Interlocked.Increment(ref c);
                return Task.CompletedTask;
            };

            try
            {
                var publishTasks = new List<Task>();
                for (int i = 0; i < n; i++)
                {
                    publishTasks.Add(ch.BasicPublishAsync("", queueName, _encoding.GetBytes("msg")).AsTask());
                }
                await Task.WhenAll(publishTasks).WaitAsync(ShortSpan);

                // Note: number of event invocations is not guaranteed
                // to be equal to N because acks can be batched,
                // so we primarily care about event handlers being invoked
                // in this test
                Assert.True(c >= 1);
            }
            finally
            {
                await ch.QueueDeleteAsync(queue: queueName, ifUnused: false, ifEmpty: false);
                await ch.CloseAsync();
            }
        }
    }
}
