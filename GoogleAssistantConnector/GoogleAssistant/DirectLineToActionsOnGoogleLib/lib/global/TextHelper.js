
 class TextHelper {

    static htmlToMarkdown(text) {
        text = text.replace(/  +/g, ' ');
        text = text.replace(/\r\n/g, '\n');
        text = text.replace(/\n/g, '  \n');

        text = text.replace(/<b>/g, '**');
        text = text.replace(/<\/b>/g, '**');

        text = text.replace(/<i>/g, '*');
        text = text.replace(/<\/i>/g, '*');
       
        return text;
    }

     static cleanText(text, withCarryReturns = true) {
         let carryReturn = (withCarryReturns) ? '  \n' : '';

         text = text.replace(/  +/g, ' ');
         text = text.replace(/\r\n/g, carryReturn);
         text = text.replace(/\n/g, carryReturn);

         return text;
     }

     static cleanMarkdown(text) {
         if (text) {
             text = text.replace(/\**/g, '');
             text = text.replace(/\*/g, '');
         }
         return text;
     }

     static secureUrl(url) {
         if (url) {
             url = url.replace('http://', 'https://');
         }
         return url;
     }
};

module.exports = TextHelper;

