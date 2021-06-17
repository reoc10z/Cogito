### THIS SCRIPT IS GOING TO ANALYZE DATA IN ORDER TO TEST HYPOTHESIS FROM 
### VARIABLE: ball-time_ms


### libraries installed

# library to manage data
library(dplyr)

# library for plots
library(ggpubr)

#library for some statistical tests
library(car)


### read data
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'
input_file_ball = 'ball_data.csv'

in_data_ball <- read.csv( paste(input_path,input_file_ball, sep ='') , sep = ';' )
# Transform none to na
in_data_ball <- na_if(in_data_ball, 'None')
# transform to factor, logical and numeric values
in_data_ball$type_test <- as.factor(in_data_ball$type_test)
in_data_ball[, 6]  <- as.logical(in_data_ball[, 6])
in_data_ball[, c(1, 3:5, 7:8)]  = as.numeric( unlist( in_data_ball[, c(1, 3:5, 7:8)] ) )
str(in_data_ball)

########
######## IF DECIDED, USE THIS OPTION:
######## replace nans in total_time-ms for value 2250ms (greatest possible value for total_time_ms)
######## 
#
# in_data_ball$time_ms[is.na(in_data_ball$time_ms)] <- 2250
# in_data_ball
#
##########

### Independence of data (1/3 anova assumptions)
# Criterion based on experiment design 

### TEST NORMALITY (2/3 anova assumptions)
# to apply anova, it REQUIRES that data has a gaussian behavior 
# histogram for all values of all users and all levels for reaction time in ms
hist(  in_data_ball[ in_data_ball$level==0, ]$time_ms )
hist(  in_data_ball[ in_data_ball$level==1, ]$time_ms )
hist(  in_data_ball[ in_data_ball$level==2, ]$time_ms )

# Compute Shapiro-Wilk test of normality
# I expect a big p-value to assume normality
# if your sample size is greater than 50, the normal QQ plot is preferred because at larger sample sizes 
# the Shapiro-Wilk test becomes very sensitive even to a minor deviation from normality.
# --> SHAPIRO TEST IS NOS USABLE FOR ME
# shapiro.test( in_data_ball[ in_data_ball$level==0, ]$time_ms )

# qq plot to test normality

# qqplot by levels
ggqqplot(  in_data_ball[ in_data_ball$level==0, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$level==1, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$level==2, ]$time_ms )

# qqplots by type of test in level 0
level = 0
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Base' & in_data_ball$level == level, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Auditory' & in_data_ball$level == level, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Haptic' & in_data_ball$level == level, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' HapticAuditory' & in_data_ball$level == level, ]$time_ms )

# qqplots by type of test in level 1
level = 1
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Base' & in_data_ball$level == level, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Auditory' & in_data_ball$level == level, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Haptic' & in_data_ball$level == level, ]$time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' HapticAuditory' & in_data_ball$level == level, ]$time_ms )


# Box plots by type of tests. Level 0
ggboxplot(in_data_ball[ in_data_ball$level==0, ] , x = "type_test", y = "time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "time_ms", xlab = "Test type", title = "time_ms in LEVEL 0" ,
)

# Box plots by type of tests. Level 1
ggboxplot(in_data_ball[ in_data_ball$level==1, ] , x = "type_test", y = "time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "time_ms", xlab = "Test type", title = "time_ms in LEVEL 1" ,
)

# Box plots by type of tests. Level 2
ggboxplot(in_data_ball[ in_data_ball$level==2, ] , x = "type_test", y = "time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "time_ms", xlab = "Test type", title = "time_ms in LEVEL 2" ,
)

### Test homogeneity of variance (3/3 anova assumptions)
# Using the residuals versus fits plot, I EXPECT not to meet a pattern in the graph, what means residuals are not related (good thing)
# Using the leveneTest I expect a big p-value (a stat page says greater than 0.05 is okay). H0: the population variances are equal (homogeneity of variance or homoscedasticity)
# level 0
res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0), ] )
plot(res.aov, 1)
leveneTest(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0), ] )

# level 1
res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(1), ] )
plot(res.aov, 1)
leveneTest(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(1), ] )

# level 2
res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(2), ] )
plot(res.aov, 1)
leveneTest(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(2), ] )


### ONE-WAY ANOVA TEST: +info: http://www.sthda.com/english/wiki/one-way-anova-test-in-r
# Null hypothesis: the means of the different groups are the same
# Alternative hypothesis: At least one sample mean is not equal to the others.
# if p-value is small, it means there is at least one group with a mean value with a significant difference

# according to the type of test for level 0
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0), ] )
summary(res.aov)

# according to the type of test for level 1
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(1), ] )
summary(res.aov)

## according to the type of test for levels 0 and 1
## I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0,1), ] )
# summary(res.aov)

# according to the type of test for level 2
# I am expecting that all means have a significant difference, i.e. a small p-value
res.aov <- aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==2, ] )
summary(res.aov)

# in level 2 make anova comparison pairwise
# Tukey multiple pairwise-comparisons. + info: http://www.sthda.com/english/wiki/one-way-anova-test-in-r
#
# tukey's assumptions:
# 1- The observations being tested are independent within and among the groups.
# 2- The groups associated with each mean in the test are normally distributed.
# 3- There is equal within-group variance across the groups associated with each mean in the test (homogeneity of variance). (Use the levenes test)
#     --> to test 3, the levenes test: 
#                   - the null hypothesis is that all populations variances are equal;
#                   - the alternative hypothesis is that at least two of them differ.
#         For levenes test, I expect that I cannot reject the null H, i.e. a big p-value
leveneTest(time_ms ~ type_test , data = in_data_ball[ in_data_ball$level==2, ])

# Homogneity of variance assumption: based on https://www.datanovia.com/en/lessons/anova-in-r/#check-assumptions
# The residuals versus fits plot can be used to check the homogeneity of variances
# I expect there is no any pattern in the plot, what means residuals are not related (good thing)
model = lm(time_ms ~ type_test , data = in_data_ball[ in_data_ball$level==1 , ])
plot( model , 1)

# diff: difference between means of the two groups
# lwr, upr: the lower and the upper end point of the confidence interval at 95% (default)
# p adj: p-value after adjustment for the multiple comparisons.
# a small p-value implies that that diff is significant
TukeyHSD(res.aov)

summary( aov(time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==2, ] ) )

## In some part I read TukeyTest is better than t-test. Thus, I am NOT GONNA do t-test for comparison
## t-test to see if a group has a greater mean value than the other
## 'greater' means Ha: average-group1 > average-group2
## 'less'    means Ha: average-group1 < average-group2
#
#versus = c(' Base', ' Auditory')
#t.test( in_data_ball[ in_data_ball$type_test == versus[1] & in_data_ball$level==2, ]$time_ms,
#        in_data_ball[ in_data_ball$type_test == versus[2] & in_data_ball$level==2, ]$time_ms,
#        alternative = "greater"
#)




