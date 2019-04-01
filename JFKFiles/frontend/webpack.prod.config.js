const webpack = require("webpack");
const path = require("path");
const webpackMerge = require("webpack-merge");
const commonConfig = require("./webpack.base.config.js");
const ExtractTextPlugin = require("extract-text-webpack-plugin");

const basePath = __dirname;

module.exports = function () {
  return webpackMerge(commonConfig, {
    devtool: "none",

    output: {
      path: path.join(basePath, "dist"),
      filename: "[chunkhash].[name].js"
    },

    module: {
      rules: [
        // *** Loading pipe for vendor CSS. No CSS Modules here ***
        {
          test: /\.css$/,
          include: [/node_modules/],
          loader: ExtractTextPlugin.extract({
            fallback: "style-loader",
            use: [
              {
                loader: "css-loader",               
              },
            ]
          })
        },
        // *** Loading pipe for SASS stylesheets ***
        {
          test: /\.scss$/,
          exclude: [/node_modules/],
          loader: ExtractTextPlugin.extract({
            fallback: "style-loader",
            use: [
              {
                loader: "css-loader",
                options: {
                  modules: true,
                  camelCase: true,
                  importLoaders: 1,
                  localIdentName: "[local]__[name]___[hash:base64:5]"
                }
              },
              { loader: 'resolve-url-loader' },
              { loader: "sass-loader" }
            ]
          })
        }
      ]
    },
    
    plugins: [
      new ExtractTextPlugin({
        filename: "[chunkhash].[name].css",
        disable: false,
        allChunks: true
      }),
      new webpack.DefinePlugin({
        "process.env": {
          DEBUG_TRACES: false
        }
      })
    ],
  });
};
