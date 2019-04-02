const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

const basePath = __dirname;

module.exports = {
  context: path.join(basePath, "src"),
 
  resolve: {
      extensions: ['.js', '.jsx', '.ts', '.tsx']
  },

  entry: {
    app: [
      './app.tsx',
    ],
    appStyles: [
      './theme/main.scss',
    ],
    vendor: [
      'babel-polyfill',
      'material-ui',
      'material-ui-icons',
      'material-ui-pickers',
      'moment',
      'paginator',
      'qs',
      'react',
      'react-dom',
      'react-router-dom',
      'd3'
    ],
  },

  module: {
    rules: [
      // *** Loading pipe for Typescript ***
      {
        test: /\.(ts|tsx)$/,
        exclude: [/node_modules/],
        loader: 'awesome-typescript-loader',
        options: {
          useBabel: true,
        },
      },
       // *** Loading pipe for Raster Images ***
       {
        test: /\.(png|jpg|gif|bmp)?$/,
        exclude: [/node_modules/],
        use: [
          {
            loader: "url-loader",
            options: {
              limit: 16000,
              name: "assets/img/[name].[ext]",
            },
          },
        ],
      },
      // *** Loading pipe for Vector Images (exclude svg from fonts) ***
      {
        test: /\.svg$/,
        exclude: [/node_modules/, /fonts/],
        use: [
          {
            loader: "url-loader",
            options: {
              limit: 1000,
              name: "assets/svg/[name].[ext]",
            },
          },
        ],
      },
      // *** Loading pipe for Fonts. Primary EOT (welcome to be embedded).
      // The rest are fallbacks just in case, dont' embedd them ***
      {
        test: /\.eot$/,
        // exclude: [/node_modules/],
        use: [
          {
            loader: "url-loader",
            options: {
              limit: 5000,
              mimetype: "application/vnd.ms-fontobject",
              name: "assets/fonts/[name].[ext]",
            },
          },
        ],
      },
      {
        test: /\.([ot]tf)$/,
        // exclude: [/node_modules/],
        use: [
          {
            loader: "url-loader",
            options: {
              limit: 1000,
              mimetype: "application/octet-stream",
              name: "assets/fonts/[name].[ext]",
            },
          },
        ],
      },
      {
        test: /\.(woff|woff2)?$/,
        // exclude: [/node_modules/],
        use: [
          {
            loader: "url-loader",
            options: {
              limit: 1000,
              mimetype: "mimetype=application/font-woff",
              name: "assets/fonts/[name].[ext]",
            },
          },
        ],
      },
      {
        test: /\.svg$/,
        // exclude: [/node_modules/],
        include: [/fonts/],
        use: [
          {
            loader: "url-loader",
            options: {
              limit: 1000,
              mimetype: "image/svg+xml",
              name: "assets/fonts/[name].[ext]",
            },
          },
        ],
      },      
    ]
  },
  plugins: [
    // *** Generate index.html in /dist ***
    new HtmlWebpackPlugin({
      filename: 'index.html', // Name of file in ./dist/
      template: 'index.html', // Name of template in ./src
      hash: true,
      chunksSortMode: 'manual',
      chunks: ['manifest', 'vendor', 'appStyles', 'app'],
    }),
    new webpack.optimize.CommonsChunkPlugin({
      names: ['appStyles', 'vendor', 'manifest'],
    }),
    new webpack.DefinePlugin({
      'process.env': {
        'SEARCH_CONFIG_PROTOCOL': JSON.stringify(process.env.SEARCH_CONFIG_PROTOCOL),
        'SEARCH_CONFIG_SERVICE_NAME': JSON.stringify(process.env.SEARCH_CONFIG_SERVICE_NAME),
        'SEARCH_CONFIG_SERVICE_DOMAIN': JSON.stringify(process.env.SEARCH_CONFIG_SERVICE_DOMAIN),
        'SEARCH_CONFIG_SERVICE_PATH': JSON.stringify(process.env.SEARCH_CONFIG_SERVICE_PATH),
        'SEARCH_CONFIG_API_VER': JSON.stringify(process.env.SEARCH_CONFIG_API_VER),
        'SEARCH_CONFIG_API_KEY': JSON.stringify(process.env.SEARCH_CONFIG_API_KEY),
        'SUGGESTION_CONFIG_PROTOCOL': JSON.stringify(process.env.SUGGESTION_CONFIG_PROTOCOL),
        'SUGGESTION_CONFIG_SERVICE_NAME': JSON.stringify(process.env.SUGGESTION_CONFIG_SERVICE_NAME),
        'SUGGESTION_CONFIG_SERVICE_DOMAIN': JSON.stringify(process.env.SUGGESTION_CONFIG_SERVICE_DOMAIN),
        'SUGGESTION_CONFIG_SERVICE_PATH': JSON.stringify(process.env.SUGGESTION_CONFIG_SERVICE_PATH),
        'SUGGESTION_CONFIG_API_VER': JSON.stringify(process.env.SUGGESTION_CONFIG_API_VER),
        'SUGGESTION_CONFIG_API_KEY': JSON.stringify(process.env.SUGGESTION_CONFIG_API_KEY),
        'FUNCTION_CONFIG_PROTOCOL': JSON.stringify(process.env.FUNCTION_CONFIG_PROTOCOL),
        'FUNCTION_CONFIG_SERVICE_NAME': JSON.stringify(process.env.FUNCTION_CONFIG_SERVICE_NAME),
        'FUNCTION_CONFIG_SERVICE_DOMAIN': JSON.stringify(process.env.FUNCTION_CONFIG_SERVICE_DOMAIN),
        'FUNCTION_CONFIG_SERVICE_PATH': JSON.stringify(process.env.FUNCTION_CONFIG_SERVICE_PATH),
        'FUNCTION_CONFIG_SERVICE_AUTH_CODE_PARAM': JSON.stringify(process.env.FUNCTION_CONFIG_SERVICE_AUTH_CODE_PARAM)
      }
    }),
    new CopyWebpackPlugin([
      {context: "assets/favicon", from: "**/*"}
    ])
  ]
}
