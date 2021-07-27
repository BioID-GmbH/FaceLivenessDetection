/// <binding BeforeBuild='build' Clean='clean' />

'use strict';

const { src, dest, series, parallel } = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const postcss = require('gulp-postcss');
const autoprefixer = require('autoprefixer');
const del = require('del');

const paths = {
    sass: ["./wwwroot/scss/bioid.scss"],
    cssDest: "./wwwroot/css/",
    css: "./wwwroot/css/**/*.css",
    delCss: ["./wwwroot/css/**/*.min.css"],
    js: "./wwwroot/js/**/*.js",
    delJs: ["./wwwroot/js/**/*.min.js"]
};

function cleanCss() { return del(paths.delCss); }
function cleanJs() { return del(paths.delJs); }
const clean = parallel(cleanCss, cleanJs);

function scss() {
    // sass compiler with autoprefixer
    return src(paths.sass)
        .pipe(sass())
        .pipe(postcss([autoprefixer()]))
        .pipe(dest(paths.cssDest));
}

exports.build = series(scss);
exports.clean = clean;
exports.scss = scss;
