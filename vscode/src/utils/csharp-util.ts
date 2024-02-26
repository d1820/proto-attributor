import { EndOfLine, TextDocument, TextEditor } from 'vscode';
import { IWindow } from '../interfaces/window.interface';
import { match } from 'assert';

export type PublicProtected = 'public' | 'protected';

export enum SignatureType
{
  FullProperty,
  LambaProperty,
  Method,
  Enum,
  Class,
  Unknown
}

export class SignatureLineResult
{
  signature: string | null;
  signatureType: SignatureType;
  lineMatchStartsOn: number;
  defaultLineIndent: number;
  leadingTrivia: string[] | null;

  constructor(signature: string | null, signatureType: SignatureType, lineMatchStartsOn: number)
  {
    this.signature = signature;
    this.signatureType = signatureType;
    this.lineMatchStartsOn = lineMatchStartsOn;
  }
}

export const getEnumBody = (text: string): string[] =>
{
  const regEx = new RegExp('public enum[^\\}]*\\{([^\\}]*)\\}', 'gm');
  const body = regEx.exec(text);
  if (body!.length>1)
  {
    return body![1].split(',');
  }
  return [];

};

export const getLeadingTrivia = (document: TextDocument, finalSig: SignatureLineResult): void =>
{
  let preSignatureStartingLine = finalSig.lineMatchStartsOn - 1; //start at the line just above the signature
  let preSignatureText = [];
  let preSignatureLine: string | null = '';
  while (true)
  {
    preSignatureLine = (document.lineAt(preSignatureStartingLine).text || '').trim();

    if (preSignatureLine.startsWith('///') || preSignatureLine.startsWith('['))
    {
      preSignatureText.push(preSignatureLine);
    }
    if (preSignatureLine.startsWith('using') //this would be hitting top of file
      || preSignatureLine.startsWith('}') //this would be hitting next method or full property up
      || preSignatureLine.indexOf(';') > -1) //this would be hitting lambda property line above
    {
      break;
    }
    preSignatureStartingLine--; //go up a line at a time
  }
  preSignatureText = preSignatureText.reverse();
  finalSig.leadingTrivia = preSignatureText;
};

export const getAllPublicMembers = (text: string, document: TextDocument): SignatureLineResult[] =>
{
  const members: SignatureLineResult[] = [];
  // Strip out `abstract ` modifier
  const matches = text.match(/public.*/g);
  const textLines = text.split('\n');
  matches?.forEach(match =>
  {
    let sig: SignatureLineResult;
    const line = match;
    const lineNumber = textLines.findIndex(line => line.includes(match));

    if (line.indexOf('class') > -1)
    {
      sig = new SignatureLineResult(line, SignatureType.Class, lineNumber);
    }
    else if (line.indexOf('enum') > -1)
    {
      sig = new SignatureLineResult(line, SignatureType.Enum, lineNumber);
    }
    else if (line.indexOf('(') > -1)
    {
      sig = new SignatureLineResult(line, SignatureType.Method, lineNumber);
    }
    else if (line.indexOf('=>') > -1)
    {
      sig = new SignatureLineResult(line, SignatureType.LambaProperty, lineNumber);
    }
    else
    {
      sig = new SignatureLineResult(line, SignatureType.FullProperty, lineNumber);
    }
    getLeadingTrivia(document, sig);
    sig.defaultLineIndent = getBeginningOfLineIndent(textLines[lineNumber]);
    members.push(sig);
  });
  console.log(members);
  return members;
};

export const getNamespace = (text: string, window: IWindow): string | null =>
{
  // Search for words after "namespace".
  const namespace = text.match(/(?<=\bnamespace\s)(.+)/);
  if (!namespace)
  {
    window.showErrorMessage('Could not find the namespace.');
    return null;
  }
  return namespace[0];
};

export const getClassName = (text: string, window: IWindow): string | null =>
{
  // Strip out `abstract ` modifier
  text = text.replace(/abstract /g, '');
  // Search for the first word after "public class" to find the name of the model.
  const classNames = text.match(/(?<=\bpublic class\s)(\w+)/);
  if (!classNames)
  {
    window.showErrorMessage('Could not find the class name.');
    return null;
  }
  return classNames[0];
};

export const getMemberName = (text: string): string | undefined =>
{
  const memberRegEx = new RegExp('\\w*.*(?=[\\{\\(])', 'gm');
  // Search for the first word after "public class" to find the name of the model.
  const memberNameMatches = text.match(memberRegEx);
  if (!memberNameMatches)
  {
    return undefined;
  }
  const name = memberNameMatches[0].trim().split(' ').pop();
  return name;
};

export const cleanString = (str: string | null): string | null =>
{
  if (!str)
  {
    return str;
  }
  const regex = /\s{2,}[\r\n]*/gm;
  return str.replace(regex, '').trim();
};

export const getLineEndingFromDoc = (document: TextDocument): string =>
{
  if (EndOfLine.CRLF === document.eol)
  {
    return '\r\n';
  }
  return '\n';
};

export const getUsingStatements = (editor: TextEditor, eol: string): string[] =>
{
  const document = editor.document;
  const docText = document.getText();
  return getUsingStatementsFromText(docText, eol);
};

export const getUsingStatementsFromText = (docText: string, eol: string): string[] =>
{
  let lines = docText.split(eol);
  return lines.filter(f => f.startsWith('using'));
};

export const replaceUsingStatementsFromText = (docText: string, newUsings: string[], eol: string): string =>
{
  let lines = docText.split(eol);
  lines = lines.filter(f => !f.startsWith('using'));
  lines = [...newUsings, ...lines];
  return lines.join(eol);
};

export const getBeginningOfLineIndent = (text: string): number =>
{
  const spaceCountRegex = /^[\r\n]*(\s*)/;
  const count = text.match(spaceCountRegex);
  if (count && count.length > 1)
  {
    return count[1].length;
  }
  return 0;
};

