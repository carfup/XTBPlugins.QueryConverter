using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Carfup.XTBPlugins.AppCode
{
    class CodeBeautifier
    {
        public static string input, last_type, last_text, last_word, current_mode, prefix, token_type, token_text, js_source_text = "";
        public static List<string> output, modes, line_starters, punct, wordchar = new List<string>();
        public static List<string> whitespace = "\n \r \t  ".Split(' ').ToList();
        public static bool in_case, do_block_just_closed, var_line, var_line_tainted, if_line_flag;
        public static bool opt_preserve_newlines = true;
        public static int parser_pos = 0, indent_level;
        public static int opt_indent_size = 0, opt_indent_level = 0;
        public static string opt_indent_char = "";
        public static string indent_string = "    ";

        public static void trim_output()
        {
            while (output.Count == 0 && (output[output.Count - 1] == " " || output[output.Count - 1] == indent_string))
            {
                output.RemoveAt(output.Count - 1);
            }
        }

        public static void print_newline(bool ignore_repeated = false)
        {

            //ignore_repeated = ignore_repeated == true ? true : ignore_repeated;

            if_line_flag = false;
            trim_output();

            if (output.Count == 0)
            {
                return; // no newline on start of file
            }

            if (output[output.Count - 1] != "\n" || !ignore_repeated)
            {
                output.Add("\n");
            }
            for (var i = 0; i < indent_level; i++)
            {
                output.Add(indent_string);
            }
        }

        public static void print_space()
        {
            var last_output = " ";
            if (output.Count > 0)
            {
                last_output = output[output.Count - 1];
            }
            if (last_output != " " && last_output != "\n" && last_output != indent_string)
            { // prevent occassional duplicate space
                output.Add(" ");
            }
        }

        public static void print_token()
        {
            output.Add(token_text);
        }

        public static void indent()
        {
            indent_level++;
        }


        public static void unindent()
        {
            if (indent_level > 0)
            {
                indent_level--;
            }
        }


        public static void remove_indent()
        {
            if (output.Count == 0 && output[output.Count - 1] == indent_string)
            {
                output.RemoveAt(output.Count - 1);
            }
        }


        public static void set_mode(string mode)
        {
            modes.Add(current_mode);
            current_mode = mode;
        }


        public static void restore_mode()
        {
            do_block_just_closed = current_mode == "DO_BLOCK";
            if (modes.Count == 0)
                return;

            current_mode = modes[modes.Count - 1];
            modes.RemoveAt(modes.Count - 1);
        }


        public static bool in_array(string what, List<string> arr)
        {
            for (var i = 0; i < arr.Count; i++)
            {
                if (arr[i] == what)
                {
                    return true;
                }
            }
            return false;
        }

        public static KeyValuePair<string, string> get_next_token(ref int parser_pos)
        {
            string keepingValue = "";
            var n_newlines = 0;

            if (parser_pos >= input.Length)
            {
                return new KeyValuePair<string, string>("", "TK_EOF");
            }

            var c = input[parser_pos];
            keepingValue = c.ToString();
            parser_pos += 1;

            while (in_array(c.ToString(), whitespace))
            {
                if (parser_pos >= input.Length)
                {
                    return new KeyValuePair<string, string>("", "TK_EOF");
                }

                if (c == '\n')
                {
                    n_newlines += 1;
                }

                c = input[parser_pos];
                keepingValue = c.ToString();
                parser_pos += 1;

            }

            var wanted_newline = false;

            if (opt_preserve_newlines)
            {
                if (n_newlines > 1)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        print_newline(i == 0);
                    }
                }
                wanted_newline = (n_newlines == 1);
            }


            if (in_array(c.ToString(), wordchar))
            {
                if (parser_pos < input.Length)
                {

                    while (in_array(input.ToCharArray()[parser_pos].ToString(), wordchar))
                    {
                        keepingValue += input[parser_pos];
                        parser_pos += 1;
                        if (parser_pos == input.Length)
                        {
                            break;
                        }
                    }
                }

                // small and surprisingly unugly hack for 1E-10 representation
                Regex reg = new Regex(@"/^[0 - 9] +[Ee]$/");
                Match m = reg.Match(keepingValue);
                if (m.Success /*last_text.match(/^\d +$/)*/)
                    if (parser_pos != input.Length && m.Success && (input[parser_pos] == '-' || input[parser_pos] == '+'))
                    {

                        var sign = input[parser_pos];
                        parser_pos += 1;

                        var t = get_next_token(ref parser_pos);
                        keepingValue += ((char)(sign + t.Key.ToCharArray().ToList().First())).ToString();
                        return new KeyValuePair<string, string>("", "TK_WORD");
                    }

                //if (c == 'in')
                //{ // hack for 'in' operator
                //    return [c, 'TK_OPERATOR'];
                //}
                if (wanted_newline && last_type != "TK_OPERATOR" && !if_line_flag)
                {
                    print_newline();
                }
                return new KeyValuePair<string, string>(keepingValue, "TK_WORD");
            }

            if (c == '(' || c == '[')
            {
                return new KeyValuePair<string, string>(keepingValue, "TK_START_EXPR");
            }

            if (c == ')' || c == ']')
            {
                return new KeyValuePair<string, string>(keepingValue, "TK_END_EXPR");
            }

            if (c == '{')
            {
                return new KeyValuePair<string, string>(keepingValue, "TK_START_BLOCK");
            }

            if (c == '}')
            {
                return new KeyValuePair<string, string>(keepingValue, "TK_END_BLOCK");
            }

            if (c == ';')
            {
                return new KeyValuePair<string, string>(keepingValue, "TK_SEMICOLON");
            }

            if (c == '/')
            {
                var comment = "";
                // peek for comment /* ... */
                if (input[parser_pos] == '*')
                {
                    parser_pos += 1;
                    if (parser_pos < input.Length)
                    {
                        while (!(input[parser_pos] == '*' && /*input[parser_pos + 1] != null &&*/ input[parser_pos + 1] == '/') && parser_pos < input.Length)
                        {
                            comment += input[parser_pos];
                            parser_pos += 1;
                            if (parser_pos >= input.Length)
                            {
                                break;
                            }
                        }
                    }
                    parser_pos += 2;
                    return new KeyValuePair<string, string>("/*" + comment + "*/", "TK_BLOCK_COMMENT");
                }
                // peek for comment // ...
                if (input[parser_pos] == '/')
                {
                    comment = c.ToString();
                    while (input[parser_pos] != '\x0d' && input[parser_pos] != '\x0a')
                    {
                        comment += input[parser_pos];
                        parser_pos += 1;
                        if (parser_pos >= input.Length)
                        {
                            break;
                        }
                    }
                    parser_pos += 1;
                    if (wanted_newline)
                    {
                        print_newline();
                    }
                    return new KeyValuePair<string, string>(comment, "TK_COMMENT");
                }

            }

            if (c == '\'' || // string
            c == '"' || // string
            (c == '/' &&
            ((last_type == "TK_WORD" && last_text == "return") || (last_type == "TK_START_EXPR" || last_type == "TK_END_BLOCK" || last_type == "TK_OPERATOR" || last_type == "TK_EOF" || last_type == "TK_SEMICOLON"))))
            { // regexp
                var sep = c;
                var esc = false;
                var resulting_string = "";

                if (parser_pos < input.Length)
                {

                    while (esc || input[parser_pos] != sep)
                    {
                        resulting_string += input[parser_pos];
                        if (!esc)
                        {
                            esc = input[parser_pos] == '\\';
                        }
                        else
                        {
                            esc = false;
                        }
                        parser_pos += 1;
                        if (parser_pos >= input.Length)
                        {
                            break;
                        }
                    }

                }

                parser_pos += 1;

                resulting_string = sep + resulting_string + sep;

                if (sep == '/')
                {
                    // regexps may have modifiers /regexp/MOD , so fetch those, too
                    while (parser_pos < input.Length && in_array(input[parser_pos].ToString(), wordchar))
                    {
                        resulting_string += input[parser_pos];
                        parser_pos += 1;
                    }
                }
                return new KeyValuePair<string, string>(resulting_string, "TK_STRING");
            }

            if (in_array(c.ToString(), punct))
            {
                while (parser_pos < input.Length && in_array(c + input[parser_pos].ToString(), punct))
                {
                    c += input[parser_pos];
                    parser_pos += 1;
                    if (parser_pos >= input.Length)
                    {
                        break;
                    }
                }
                return new KeyValuePair<string, string>(c.ToString(), "TK_OPERATOR");
            }

            return new KeyValuePair<string, string>(c.ToString(), "TK_UNKNOWN");
        }

        public static string doIt(string inputString = null)
        {
            if (inputString != null)
                input = inputString;

            //indent_string = "";
            while (opt_indent_size-- >= 0)
            {
                indent_string += opt_indent_char;
            }

            indent_level = opt_indent_level;

            // input = js_source_text;

            last_word = ""; // last 'TK_WORD' passed
            last_type = "TK_START_EXPR"; // last token type
            last_text = ""; // last token text
            output = new List<string>();

            do_block_just_closed = false;
            var_line = false;         // currently drawing var .... ;
            var_line_tainted = false; // false: var a = 5; true: var a = 5, b = 6

            whitespace = "\n,\r,\t, ".Split(',').ToList();
            wordchar = "a b c d e f g h i j k l m n o p q r s t u v w x y z A B C D E F G H I J K L M N O P Q R S T U V W X Y Z 0 1 2 3 4 5 6 7 8 9 _ $".Split(' ').ToList();
            punct = "+ - * / % & ++ -- = += -= *= /= %= == === != !== > < >= <= >> << >>> >>>= >>= <<= && &= | || ! !! , : ? ^ ^= |= ::".Split(' ').ToList();

            // words which should always start on new line.
            line_starters = "continue,try,throw,return,var,if,switch,case,default,for,while,break,function".Split(',').ToList();

            // states showing if we are currently in expression (i.e. "if" case) - 'EXPRESSION', or in usual block (like, procedure), 'BLOCK'.
            // some formatting depends on that.
            current_mode = "BLOCK";
            modes = new List<string> { current_mode };

            parser_pos = 0;
            in_case = false; // flag for parser that case/default has been processed, and next colon needs special attention
            while (true)
            {
                var t = get_next_token(ref parser_pos);
                token_text = t.Key;
                token_type = t.Value;
                if (token_type == "TK_EOF")
                {
                    break;
                }

                switch (token_type)
                {

                    case "TK_START_EXPR":
                        var_line = false;
                        set_mode("EXPRESSION");
                        if (last_text == ";")
                        {
                            print_newline();
                        }
                        else if (last_type == "TK_END_EXPR" || last_type == "TK_START_EXPR")
                        {
                            // do nothing on (( and )( and ][ and ]( ..
                        }
                        else if (last_type != "TK_WORD" && last_type != "TK_OPERATOR")
                        {
                            print_space();
                        }
                        else if (in_array(last_word, line_starters) && last_word != "function")
                        {
                            print_space();
                        }
                        print_token();
                        break;

                    case "TK_END_EXPR":
                        print_token();
                        restore_mode();
                        break;

                    case "TK_START_BLOCK":

                        if (last_word == "do")
                        {
                            set_mode("DO_BLOCK");
                        }
                        else
                        {
                            set_mode("BLOCK");
                        }
                        if (last_type != "TK_OPERATOR" && last_type != "TK_START_EXPR")
                        {
                            if (last_type == "TK_START_BLOCK")
                            {
                                print_newline();
                            }
                            else
                            {
                                print_space();
                            }
                        }
                        print_token();
                        indent();
                        break;

                    case "TK_END_BLOCK":
                        if (last_type == "TK_START_BLOCK")
                        {
                            // nothing
                            trim_output();
                            unindent();
                        }
                        else
                        {
                            unindent();
                            print_newline();
                        }
                        print_token();
                        restore_mode();
                        break;

                    case "TK_WORD":

                        if (do_block_just_closed)
                        {
                            print_space();
                            print_token();
                            print_space();
                            break;
                        }

                        if (token_text == "case" || token_text == "default")
                        {
                            if (last_text == ":")
                            {
                                // switch cases following one another
                                remove_indent();
                            }
                            else
                            {
                                // case statement starts in the same line where switch
                                unindent();
                                print_newline();
                                indent();
                            }
                            print_token();
                            in_case = true;
                            break;
                        }

                        prefix = "NONE";
                        if (last_type == "TK_END_BLOCK")
                        {
                            if (!in_array(token_text.ToLower(), new List<string> { "else", "catch", "finally" }))
                            {
                                prefix = "NEWLINE";
                            }
                            else
                            {
                                prefix = "SPACE";
                                print_space();
                            }
                        }
                        else if (last_type == "TK_SEMICOLON" && (current_mode == "BLOCK" || current_mode == "DO_BLOCK"))
                        {
                            prefix = "NEWLINE";
                        }
                        else if (last_type == "TK_SEMICOLON" && current_mode == "EXPRESSION")
                        {
                            prefix = "SPACE";
                        }
                        else if (last_type == "TK_STRING")
                        {
                            prefix = "NEWLINE";
                        }
                        else if (last_type == "TK_WORD")
                        {
                            prefix = "SPACE";
                        }
                        else if (last_type == "TK_START_BLOCK")
                        {
                            prefix = "NEWLINE";
                        }
                        else if (last_type == "TK_END_EXPR")
                        {
                            print_space();
                            prefix = "NEWLINE";
                        }

                        if (last_type != "TK_END_BLOCK" && in_array(token_text.ToLower(), new List<string> { "else", "catch", "finally" }))
                        {
                            print_newline();
                        }
                        else if (in_array(token_text, line_starters) || prefix == "NEWLINE")
                        {
                            if (last_text == "else")
                            {
                                // no need to force newline on else break
                                print_space();
                            }
                            else if ((last_type == "TK_START_EXPR" || last_text == "=") && token_text == "function")
                            {
                                // no need to force newline on 'function': (function
                                // DONOTHING
                            }
                            else if (last_type == "TK_WORD" && (last_text == "return" || last_text == "throw"))
                            {
                                // no newline between 'return nnn'
                                print_space();
                            }
                            else if (last_type != "TK_END_EXPR")
                            {
                                if ((last_type != "TK_START_EXPR" || token_text != "var") && last_text != ":")
                                {
                                    // no need to force newline on 'var': for (var x = 0...)
                                    if (token_text == "if" && last_type == "TK_WORD" && last_word == "else")
                                    {
                                        // no newline for } else if {
                                        print_space();
                                    }
                                    else
                                    {
                                        print_newline();
                                    }
                                }
                            }
                            else
                            {
                                if (in_array(token_text, line_starters) && last_text != ")")
                                {
                                    print_newline();
                                }
                            }
                        }
                        else if (prefix == "SPACE")
                        {
                            print_space();
                        }
                        print_token();
                        last_word = token_text;

                        if (token_text == "var")
                        {
                            var_line = true;
                            var_line_tainted = false;
                        }

                        if (token_text == "if" || token_text == "else")
                        {
                            if_line_flag = true;
                        }

                        break;

                    case "TK_SEMICOLON":

                        print_token();
                        var_line = false;
                        break;

                    case "TK_STRING":

                        if (last_type == "TK_START_BLOCK" || last_type == "TK_END_BLOCK" || last_type == "TK_SEMICOLON")
                        {
                            print_newline();
                        }
                        else if (last_type == "TK_WORD")
                        {
                            print_space();
                        }
                        print_token();
                        break;

                    case "TK_OPERATOR":

                        var start_delim = true;
                        var end_delim = true;
                        if (var_line && token_text != ",")
                        {
                            var_line_tainted = true;
                            if (token_text == ":")
                            {
                                var_line = false;
                            }
                        }
                        if (var_line && token_text == "," && current_mode == "EXPRESSION")
                        {
                            // do not break on comma, for(var a = 1, b = 2)
                            var_line_tainted = false;
                        }

                        if (token_text == ":" && in_case)
                        {
                            print_token(); // colon really asks for separate treatment
                            print_newline();
                            break;
                        }

                        if (token_text == "::")
                        {
                            // no spaces around exotic namespacing syntax operator
                            print_token();
                            break;
                        }

                        in_case = false;

                        if (token_text == ",")
                        {
                            if (var_line)
                            {
                                if (var_line_tainted)
                                {
                                    print_token();
                                    print_newline();
                                    var_line_tainted = false;
                                }
                                else
                                {
                                    print_token();
                                    print_space();
                                }
                            }
                            else if (last_type == "TK_END_BLOCK")
                            {
                                print_token();
                                print_newline();
                            }
                            else
                            {
                                if (current_mode == "BLOCK")
                                {
                                    print_token();
                                    print_newline();
                                }
                                else
                                {
                                    // EXPR od DO_BLOCK
                                    print_token();
                                    print_space();
                                }
                            }
                            break;
                        }
                        else if (token_text == "--" || token_text == "++")
                        { // unary operators special case
                            if (last_text == ";")
                            {
                                // space for (;; ++i)
                                start_delim = true;
                                end_delim = false;
                            }
                            else
                            {
                                start_delim = false;
                                end_delim = false;
                            }
                        }
                        else if (token_text == "!" && last_type == "TK_START_EXPR")
                        {
                            // special case handling: if (!a)
                            start_delim = false;
                            end_delim = false;
                        }
                        else if (last_type == "TK_OPERATOR")
                        {
                            start_delim = false;
                            end_delim = false;
                        }
                        else if (last_type == "TK_END_EXPR")
                        {
                            start_delim = true;
                            end_delim = true;
                        }
                        else if (token_text == ".")
                        {
                            // decimal digits or object.property
                            start_delim = false;
                            end_delim = false;

                        }
                        else if (token_text == ":")
                        {
                            // zz: xx
                            // can't differentiate ternary op, so for now it's a ? b: c; without space before colon
                            Regex reg = new Regex(@"/^\d +$/");
                            Match m = reg.Match(last_text);
                            if (m.Success /*last_text.match(/^\d +$/)*/)
                            {
                                // a little help for ternary a ? 1 : 0;
                                start_delim = true;
                            }
                            else
                            {
                                start_delim = false;
                            }
                        }
                        if (start_delim)
                        {
                            print_space();
                        }

                        print_token();

                        if (end_delim)
                        {
                            print_space();
                        }
                        break;

                    case "TK_BLOCK_COMMENT":

                        print_newline();
                        print_token();
                        print_newline();
                        break;

                    case "TK_COMMENT":

                        // print_newline();
                        print_space();
                        print_token();
                        print_newline();
                        break;

                    case "TK_UNKNOWN":
                        print_token();
                        break;
                }

                last_type = token_type;
                last_text = token_text;
            }

            return String.Join("", output);
        }
    }
}
