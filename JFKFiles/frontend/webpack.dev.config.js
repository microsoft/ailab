const webpack = require("webpack");
const path = require("path");
const webpackMerge = require("webpack-merge");
const commonConfig = require("./webpack.base.config.js");

const basePath = __dirname;

module.exports = function () {
  return webpackMerge(commonConfig, {
    devtool: 'source-map',

    devServer: {
         contentBase: './dist', // Content base
         inline: true, // Enable watch and live reload
         host: 'localhost',
         port: 8082,
         stats: 'errors-only'
    },

    output: {
      path: path.join(basePath, "dist"),
      filename: "[name].js"
    },

    module: {
      rules: [
        // *** Loading pipe for vendor CSS. No CSS Modules ***
        {
          test: /\.css$/,
          include: [/node_modules/],
          use: [
            "style-loader",
            {
              loader: "css-loader",              
            },
          ]
        },
        // *** Loading pipe for SASS stylesheets ***
        {
          test: /\.scss$/,
          exclude: [/node_modules/],
          use: [
            "style-loader",
            {
              loader: "css-loader",
              options: {
                modules: true,
                camelCase: true,
                sourceMap: true,
                importLoaders: 1,
                localIdentName: "[local]__[name]___[hash:base64:5]"
              }
            },
            { loader: 'resolve-url-loader' },
            "sass-loader"
          ]
        }
      ]
    },
    plugins: [
      new webpack.DefinePlugin({
        "process.env": {
          DEBUG_TRACES: true
        }
      })
    ],
  });
};
