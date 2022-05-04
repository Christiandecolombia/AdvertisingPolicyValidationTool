using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace AdvertisingPolicyValidationTool
{
    internal class CSVFileClient
    {
        private readonly string InputFileName;
        private readonly string OutputFileName;
        private readonly FileHelperAsyncEngine<DomainOutput> outputFileEngine;

        public CSVFileClient(string inputFilePath, string outputFilePath)
        {
            InputFileName = inputFilePath;
            OutputFileName = outputFilePath;
            outputFileEngine = new FileHelperAsyncEngine<DomainOutput>();
            outputFileEngine.BeginWriteFile(OutputFileName);
        }

        public List<DomainInput> ReadFile()
        {
            var records = (new FileHelperEngine<DomainInput>()).ReadFile(InputFileName);
            return records != null ? records.ToList() : new List<DomainInput>();
        }

        public void UpdateOutput(DomainOutput result)
        {
            outputFileEngine.WriteNext(result);
            outputFileEngine.Flush();
        }

        public void CloseOutput()
        {
            outputFileEngine.Close();
        }
    }
}
