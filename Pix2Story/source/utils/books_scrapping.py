import scrapy
from scrapy.selector import Selector
from scrapy.http import HtmlResponse
import urllib.parse
from scrapy.http import Request,Response


class BlogSpider(scrapy.Spider):
    name = 'bookspider'
    start_urls = ['free/book/web'+str(i) for i in range(0,10000,20)]

    def parse(self, response):
        for title in response.xpath('//a[(contains(@href,"books/view"))]'):
            url_book = response.urljoin(title.xpath('@href').extract_first())
            print('url book',url_book)
            yield Request(
                url=response.urljoin(url_book),
                callback=self.download_book)

    def download_book(self,response):
        txt_link = response.xpath('//a[(contains(@title,"Archival"))]')
        print('txtlink',txt_link)
        urltxt=txt_link.xpath('@href').extract_first()
        language = response.xpath('//li[(contains(text(),"Language"))]').extract_first()
        print(language,'language')
        if 'English'in language:
            yield Request(
                url=response.urljoin(urltxt),
                callback=self.save_txt)
        else: yield {None}
    

    def save_txt(self, response):
        path = '../books/thriller/'+response.url.split('/')[-1]
        print(path)
        with open(path, 'wb') as f:
            f.write(response.body)







