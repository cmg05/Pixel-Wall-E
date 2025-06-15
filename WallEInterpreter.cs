using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel_Wall_E
{
    public class WallEInterpreter
    {
        public class GotoException : Exception
        {
            public int TargetLine { get; }

            public GotoException(int targetLine) : base("Salto condicional ejecutado")
            {
                TargetLine = targetLine;
            }
        }

        private readonly Form1 _form;
        private readonly Dictionary<string, int> _variables = new Dictionary<string, int>();
        private readonly Dictionary<string, bool> _boolVariables = new Dictionary<string, bool>();
        private readonly Dictionary<string, int> _labels = new Dictionary<string, int>();
        private readonly Dictionary<string, Func<int>> _builtInFunctions = new Dictionary<string, Func<int>>();

        // Variables para almacenar parámetros de funciones
        private string _lastColorChecked;
        private int _lastSizeChecked;
        private int _lastVertical;
        private int _lastHorizontal;
        private bool _hasSpawned = false;

        public WallEInterpreter(Form1 form)
        {
            _form = form;
            InitializeBuiltInFunctions();
        }

        private void InitializeBuiltInFunctions()
        {
            _builtInFunctions["GetActualX"] = _form.GetActualX;
            _builtInFunctions["GetActualY"] = _form.GetActualY;
            _builtInFunctions["GetCanvasSize"] = _form.GetCanvasSize;

            // Funciones que requieren parámetros
            _builtInFunctions["IsBrushColor"] = () => _form.IsBrushColor(_lastColorChecked);
            _builtInFunctions["IsBrushSize"] = () => _form.IsBrushSize(_lastSizeChecked);
            _builtInFunctions["IsCanvasColor"] = () => _form.IsCanvasColor(_lastColorChecked, _lastVertical, _lastHorizontal);
        }

        public void ProcessLabels(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Ignorar comentarios y líneas vacías
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//")) continue;

                if (line.EndsWith(":") && !line.Contains(" "))
                {
                    string label = line.TrimEnd(':');
                    if (_labels.ContainsKey(label))
                    {
                        throw new Exception($"Etiqueta duplicada: '{label}'");
                    }
                    _labels[label] = i;
                }
            }
        }

        public void ExecuteCommand(string command, int lineNumber)
        {
            try
            {
                command = command.Trim();

                // Ignorar líneas vacías o comentarios
                if (string.IsNullOrWhiteSpace(command) || command.StartsWith("//"))
                {
                    return;
                }

                if (command.EndsWith(":"))
                {
                    return; 
                }
                else if (command.StartsWith("Spawn("))
                {
                    if (_hasSpawned)
                        throw new Exception("Spawn solo puede llamarse una vez al inicio del programa");

                    ExecuteSpawn(command);
                    _hasSpawned = true;
                }
                else if (!_hasSpawned)
                {
                    throw new Exception("El programa debe comenzar con un comando Spawn");
                }
                else if (command.StartsWith("Color("))
                {
                    ExecuteColor(command);
                }
                else if (command.StartsWith("Size("))
                {
                    ExecuteSize(command);
                }
                else if (command.StartsWith("DrawLine("))
                {
                    ExecuteDrawLine(command);
                }
                else if (command.StartsWith("DrawCircle("))
                {
                    ExecuteDrawCircle(command);
                }
                else if (command.StartsWith("DrawRectangle("))
                {
                    ExecuteDrawRectangle(command);
                }
                else if (command.Equals("Fill()", StringComparison.OrdinalIgnoreCase))
                {
                    _form.Fill();
                }
                else if (command.StartsWith("GoTo["))
                {
                    ExecuteGoto(command);
                }
                else if (command.Contains("<-")) 
                {
                    ExecuteVariableAssignment(command);
                }
                else if (command.StartsWith("GetColorCount("))
                {
                    ExecuteGetColorCount(command);
                }
                else if (command.StartsWith("IsBrushColor(") || command.StartsWith("IsBrushSize(") ||
                         command.StartsWith("IsCanvasColor("))
                {
                    ExecuteFunctionWithParams(command);
                }
                else
                {
                    throw new Exception($"Comando no reconocido: '{command}'");
                }
            }
            catch (GotoException)
            {
                throw; // Relanzar excepciones Goto
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en línea {lineNumber}: {ex.Message}");
            }
        }

        private void ExecuteSpawn(string command)
        {
            var parameters = ExtractParameters(command, 2);
            int x = EvaluateExpression(parameters[0]);
            int y = EvaluateExpression(parameters[1]);
            _form.ExecuteSpawn(x, y);
        }

        private void ExecuteColor(string command)
        {
            string color = command.Split('(', ')')[1].Replace("\"", "").Trim();
            _form.SetColor(color);
        }

        private void ExecuteSize(string command)
        {
            int size = EvaluateExpression(ExtractParameters(command, 1)[0]);
            _form.SetBrushSize(size);
        }

        private void ExecuteDrawLine(string command)
        {
            var parameters = ExtractParameters(command, 3);
            _form.DrawLine(
                EvaluateExpression(parameters[0]),
                EvaluateExpression(parameters[1]),
                EvaluateExpression(parameters[2])
            );
        }

        private void ExecuteDrawCircle(string command)
        {
            var parameters = ExtractParameters(command, 3);
            _form.DrawCircle(
                EvaluateExpression(parameters[0]),
                EvaluateExpression(parameters[1]),
                EvaluateExpression(parameters[2])
            );
        }

        private void ExecuteDrawRectangle(string command)
        {
            var parameters = ExtractParameters(command, 5);
            _form.DrawRectangle(
                EvaluateExpression(parameters[0]),
                EvaluateExpression(parameters[1]),
                EvaluateExpression(parameters[2]),
                EvaluateExpression(parameters[3]),
                EvaluateExpression(parameters[4])
            );
        }

        private void ExecuteGoto(string command)
        {
            var parts = command.Split(new[] { '[', ']', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new Exception("Formato GoTo incorrecto. Use: GoTo[etiqueta](condición)");
            }

            string label = parts[1].Trim();
            string condition = parts.Length > 2 ? parts[2].Trim() : "true";

            if (!_labels.ContainsKey(label))
            {
                throw new Exception($"Etiqueta no encontrada: '{label}'");
            }

            if (EvaluateCondition(condition))
            {
                throw new GotoException(_labels[label]);
            }
        }

        private void ExecuteVariableAssignment(string command)
        {
            var parts = command.Split(new[] { "<-" }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                throw new Exception("Asignación no válida. Formato: variable <- expresión");
            }

            string varName = parts[0].Trim();
            string expression = parts[1].Trim();

            if (!IsValidVariableName(varName))
            {
                throw new Exception($"Nombre de variable no válido: '{varName}'");
            }

            if (IsBooleanExpression(expression))
            {
                _boolVariables[varName] = EvaluateCondition(expression);
            }
            else
            {
                _variables[varName] = EvaluateExpression(expression);
            }
        }

        private void ExecuteGetColorCount(string command)
        {
            var parameters = ExtractParameters(command, 5);
            string color = parameters[0].Replace("\"", "").Trim();
            int x1 = EvaluateExpression(parameters[1]);
            int y1 = EvaluateExpression(parameters[2]);
            int x2 = EvaluateExpression(parameters[3]);
            int y2 = EvaluateExpression(parameters[4]);

            int count = _form.GetColorCount(color, x1, y1, x2, y2);
            // Esta función normalmente se usaría en una asignación
        }

        private void ExecuteFunctionWithParams(string command)
        {
            var parts = command.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            string funcName = parts[0].Trim();
            string[] parameters = parts[1].Split(',').Select(p => p.Trim()).ToArray();

            if (funcName == "IsBrushColor" && parameters.Length == 1)
            {
                _lastColorChecked = parameters[0].Replace("\"", "");
            }
            else if (funcName == "IsBrushSize" && parameters.Length == 1)
            {
                _lastSizeChecked = EvaluateExpression(parameters[0]);
            }
            else if (funcName == "IsCanvasColor" && parameters.Length == 3)
            {
                _lastColorChecked = parameters[0].Replace("\"", "");
                _lastVertical = EvaluateExpression(parameters[1]);
                _lastHorizontal = EvaluateExpression(parameters[2]);
            }
            else
            {
                throw new Exception($"Parámetros incorrectos para {funcName}");
            }
        }

        private string[] ExtractParameters(string command, int expectedCount)
        {
            int start = command.IndexOf('(') + 1;
            int end = command.LastIndexOf(')');
            string paramStr = command.Substring(start, end - start);
            string[] parameters = paramStr.Split(',').Select(p => p.Trim()).ToArray();

            if (parameters.Length != expectedCount)
            {
                throw new Exception($"Número incorrecto de parámetros. Esperados: {expectedCount}, Recibidos: {parameters.Length}");
            }

            return parameters;
        }

        private int EvaluateExpression(string expression)
        {
            expression = expression.Trim();

            // Verificar si es un número directo
            if (int.TryParse(expression, out int numericValue))
            {
                return numericValue;
            }

            // Verificar si es una variable
            if (_variables.TryGetValue(expression, out int varValue))
            {
                return varValue;
            }

            // Verificar si es una función incorporada
            if (_builtInFunctions.TryGetValue(expression, out var func))
            {
                return func();
            }

            // Evaluar expresiones matemáticas
            if (expression.Contains('+') || expression.Contains('-') ||
                expression.Contains('*') || expression.Contains('/') ||
                expression.Contains('%') || expression.Contains("**"))
            {
                return EvaluateMathExpression(expression);
            }

            throw new Exception($"No se puede evaluar la expresión: '{expression}'");
        }

        private int EvaluateMathExpression(string expression)
        {
            try
            {
                // Manejar potencias primero
                while (expression.Contains("**"))
                {
                    int index = expression.IndexOf("**");
                    int leftStart = FindOperandStart(expression, index - 1, false);
                    int rightEnd = FindOperandEnd(expression, index + 2, true);

                    string leftStr = expression.Substring(leftStart, index - leftStart);
                    string rightStr = expression.Substring(index + 2, rightEnd - (index + 2));

                    int left = EvaluateExpression(leftStr);
                    int right = EvaluateExpression(rightStr);
                    int result = (int)Math.Pow(left, right);

                    expression = expression[..leftStart] + result.ToString() + expression[rightEnd..];
                }

                // Evaluar multiplicaciones, divisiones y módulos
                expression = EvaluateMathOperations(expression, new[] { '*', '/', '%' });

                // Evaluar sumas y restas
                expression = EvaluateMathOperations(expression, new[] { '+', '-' });

                return int.Parse(expression);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al evaluar expresión matemática: '{expression}'. {ex.Message}");
            }
        }

        private string EvaluateMathOperations(string expression, char[] operators)
        {
            for (int i = 0; i < expression.Length; i++)
            {
                if (operators.Contains(expression[i]))
                {
                    int leftStart = FindOperandStart(expression, i - 1, false);
                    int rightEnd = FindOperandEnd(expression, i + 1, true);

                    string leftStr = expression.Substring(leftStart, i - leftStart);
                    string rightStr = expression.Substring(i + 1, rightEnd - (i + 1));

                    int left = EvaluateExpression(leftStr);
                    int right = EvaluateExpression(rightStr);
                    int result = expression[i] switch
                    {
                        '*' => left * right,
                        '/' => right == 0 ? throw new Exception("División por cero") : left / right,
                        '%' => left % right,
                        '+' => left + right,
                        '-' => left - right,
                        _ => throw new Exception($"Operador no soportado: {expression[i]}")
                    };

                    expression = expression[..leftStart] + result.ToString() + expression[rightEnd..];
                    i = leftStart + result.ToString().Length - 1;
                }
            }
            return expression;
        }

        private int FindOperandStart(string expression, int start, bool allowNegative)
        {
            while (start >= 0 && (char.IsDigit(expression[start]) ||
                  (allowNegative && expression[start] == '-' && (start == 0 || !char.IsDigit(expression[start - 1])))))
            {
                start--;
            }
            return start + 1;
        }

        private int FindOperandEnd(string expression, int start, bool allowNegative)
        {
            int end = start;
            if (allowNegative && end < expression.Length && expression[end] == '-')
            {
                end++;
            }
            while (end < expression.Length && char.IsDigit(expression[end]))
            {
                end++;
            }
            return end;
        }

        private bool EvaluateCondition(string condition)
        {
            condition = condition.Trim();

            // Valor booleano directo
            if (bool.TryParse(condition, out bool boolValue))
            {
                return boolValue;
            }

            // Verificar si es una variable booleana
            if (_boolVariables.TryGetValue(condition, out bool boolVar))
            {
                return boolVar;
            }

            // Manejar comparaciones
            string[] comparisonOps = { "==", "!=", ">=", "<=", ">", "<" };
            foreach (var op in comparisonOps)
            {
                if (condition.Contains(op))
                {
                    var parts = condition.Split(new[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;

                    int left = EvaluateExpression(parts[0].Trim());
                    int right = EvaluateExpression(parts[1].Trim());

                    return op switch
                    {
                        "==" => left == right,
                        "!=" => left != right,
                        ">=" => left >= right,
                        "<=" => left <= right,
                        ">" => left > right,
                        "<" => left < right,
                        _ => false
                    };
                }
            }

            // Manejar operadores lógicos
            if (condition.Contains("&&") || condition.Contains("||"))
            {
                string[] logicalOps = condition.Contains("&&") ? new[] { "&&" } : new[] { "||" };

                var parts = condition.Split(logicalOps, StringSplitOptions.RemoveEmptyEntries);
                bool result = EvaluateCondition(parts[0].Trim());

                for (int i = 1; i < parts.Length; i++)
                {
                    bool next = EvaluateCondition(parts[i].Trim());
                    result = logicalOps[0] == "&&" ? result && next : result || next;
                }

                return result;
            }

            // Verificar si es una expresión numérica que se puede convertir a booleano
            try
            {
                int numValue = EvaluateExpression(condition);
                return numValue != 0;
            }
            catch
            {
                throw new Exception($"No se puede evaluar la condición: '{condition}'");
            }
        }

        private bool IsBooleanExpression(string expression)
        {
            expression = expression.Trim();
            return expression.Contains("&&") || expression.Contains("||") ||
                   expression.Contains("==") || expression.Contains("!=") ||
                   expression.Contains(">=") || expression.Contains("<=") ||
                   expression.Contains(">") || expression.Contains("<") ||
                   (bool.TryParse(expression, out _)) ||
                   (_boolVariables.ContainsKey(expression));
        }

        private bool IsValidVariableName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // No puede comenzar con número o _
            if (char.IsDigit(name[0]) || name[0] == '_')
                return false;

            // Solo letras, números y _
            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
    }
}