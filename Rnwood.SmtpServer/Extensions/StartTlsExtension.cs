﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Rnwood.SmtpServer.Extensions
{
    public class StartTlsExtension : Extension
    {
        public override ExtensionProcessor CreateExtensionProcessor(ConnectionProcessor processor)
        {
            return new StartTlsExtensionProcessor(processor);
        }

        public override void ServerStartup(Server server)
        {

        }

        class StartTlsExtensionProcessor : ExtensionProcessor
        {
            public StartTlsExtensionProcessor(ConnectionProcessor processor)
            {
                Processor = processor;
                processor.VerbMap.SetVerbProcessor("STARTTLS", new StartTlsVerb());
            }

            public ConnectionProcessor Processor { get; private set; }

            public override string[] GetEHLOKeywords()
            {
                if (!Processor.Session.SecureConnection)
                {
                    return new[] { "STARTTLS" };
                }

                return new string[] { };
            }
        }
    }

    public class StartTlsVerb : Verb
    {
        public override void Process(ConnectionProcessor connectionProcessor, SmtpRequest request)
        {
            connectionProcessor.WriteResponse(new SmtpResponse(StandardSmtpResponseCode.ServiceReady, "Ready to start TLS"));           
            connectionProcessor.ApplyStreamFilter(stream =>
            {
                SslStream sslStream = new SslStream(stream);
                sslStream.AuthenticateAsServer(new X509Certificate(Resources.localhost));
                return sslStream;
            });

            connectionProcessor.Session.SecureConnection = true;
        }
    }
}