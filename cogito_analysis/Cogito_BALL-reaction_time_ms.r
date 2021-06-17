### THIS SCRIPT IS GOING TO ANALYZE DATA IN ORDER TO TEST HYPOTHESIS FROM 
### VARIABLE: ball-reaction_time_ms


### libraries installed

# library to manage data
# install.packages('dplyr')
library(dplyr)

# library for plots
# install.packages('ggpubr')
library(ggpubr)

#library for some statistical tests
# install.packages('car')
library(car)

### read data
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'
input_file_ball = 'ball_data.csv'

in_data_ball <- read.csv( paste(input_path,input_file_ball, sep ='') , sep = ';' )
# Transform none to na
in_data_ball <- na_if(in_data_ball, 'None')
# transform to numeric values
in_data_ball[, 6]  <- as.logical(in_data_ball[, 6])
in_data_ball[, c(1, 3:5, 7:8)]  = as.numeric( unlist( in_data_ball[, c(1, 3:5, 7:8)] ) )
in_data_ball$type_test <- as.factor(in_data_ball$type_test)
str(in_data_ball)

##
## I DECIDED TO USE THIS OPTION:
## where success failed, replace values in reaction_time_ms by na values
#
in_data_ball[ in_data_ball$success == FALSE, ]$reaction_time_ms <- NA
head(in_data_ball)
##

##########
##########
## this can help to remove outliers, people who react too fast may be was as an impulse to touch the screen and not to do the task
## mean(in_data_ball$reaction_time_ms, na.rm = TRUE ) - 2*sd(in_data_ball$reaction_time_ms, na.rm = TRUE ) 
in_data_ball$reaction_time_ms[ in_data_ball$reaction_time_ms < 80 ] <- NA

# after different tests of below eq: log( time-threshold(reaction_time_ms) ), 
# I conclude that a time threshold of 102ms is good to get a normal behavior.
# However, the levenetest did not improve to show homoscedasticity.
# Thus, I am gonna to apply only a time threshold of 80ms to remove impulse touches.
##########
##########  


### some graphs
# to apply anova, it REQUIRES that data has a gaussian behavior 
# histogram for all values of all users and all levels for reaction time in ms
# histograms by levels
hist(  in_data_ball[ in_data_ball$level==0, ]$reaction_time_ms , 20 )
hist(  in_data_ball[ in_data_ball$level==1, ]$reaction_time_ms , 20 )
hist(  in_data_ball[ in_data_ball$level==2, ]$reaction_time_ms , 20 )

# qq plot to test normality
# qqplot by levels
ggqqplot(  in_data_ball[ in_data_ball$level==0, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$level==1, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$level==2, ]$reaction_time_ms )


## Compute Shapiro-Wilk test of normality
## H0: data are normally distributed.
## I expect a big p-value to assume normality
## I do not understand why shapiro does not show normality when some histograms in $time_ms are normal
# shapiro.test(in_data_ball[ in_data_ball$level==0, ]$reaction_time_ms)
# ?shapiro.test


##########
##########
# making data to have a normal distribution
# applying log function as transformation
in_data_ball$reaction_time_ms <- log(in_data_ball$reaction_time_ms )
in_data_ball[ is.infinite(in_data_ball$reaction_time_ms), ]  <- NA

hist( in_data_ball[ in_data_ball$level==0, ]$reaction_time_ms , 100 )
hist( in_data_ball[ in_data_ball$level==1, ]$reaction_time_ms , 100 )
hist( in_data_ball[ in_data_ball$level==2, ]$reaction_time_ms  , 100 )
ggqqplot(  in_data_ball[ in_data_ball$level==0, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$level==1, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$level==2, ]$reaction_time_ms )
##########
##########

# qqplots by type of test in level 0
level = 0
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Base' & in_data_ball$level==level, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Auditory' & in_data_ball$level==level, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Haptic' & in_data_ball$level==level, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' HapticAuditory' & in_data_ball$level==level, ]$reaction_time_ms )

# qqplots by type of test in level 2
level = 2
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Base' & in_data_ball$level==level, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Auditory' & in_data_ball$level==level, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' Haptic' & in_data_ball$level==level, ]$reaction_time_ms )
ggqqplot(  in_data_ball[ in_data_ball$type_test==' HapticAuditory' & in_data_ball$level==level, ]$reaction_time_ms )

hist(  in_data_ball[ in_data_ball$type_test==' Base' & in_data_ball$level==level, ]$reaction_time_ms )
hist(  in_data_ball[ in_data_ball$type_test==' Auditory' & in_data_ball$level==level, ]$reaction_time_ms )
hist(  in_data_ball[ in_data_ball$type_test==' Haptic' & in_data_ball$level==level, ]$reaction_time_ms )
hist(  in_data_ball[ in_data_ball$type_test==' HapticAuditory' & in_data_ball$level==level, ]$reaction_time_ms )


# Box plots by type of tests. Level 0
ggboxplot(in_data_ball[ in_data_ball$level==0, ] , x = "type_test", y = "reaction_time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Test type", title = "Response time in LEVEL 0" ,
          )

# Box plots by type of tests. Level 1
ggboxplot(in_data_ball[ in_data_ball$level==1, ] , x = "type_test", y = "reaction_time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Test type", title = "Response time in LEVEL 1" ,
)

# Box plots by type of tests. Level 2
ggboxplot(in_data_ball[ in_data_ball$level==2, ] , x = "type_test", y = "reaction_time_ms", 
          color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Test type", title = "Response time in LEVEL 2" ,
)

### Test homogeneity of variance (3/3 anova assumptions)
# Using the residuals versus fits plot, I EXPECT not to meet a pattern in the graph, what means residuals are not related (good thing)
# Using the leveneTest I expect a big p-value (a stat page says greater than 0.05 is okay). H0: the population variances are equal (homogeneity of variance or homoscedasticity)
# level 0
res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0), ] )
plot(res.aov, 1)
leveneTest(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0), ] )

# level 1
res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(1), ] )
plot(res.aov, 1)
leveneTest(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(1), ] )

# level 2
res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(2), ] )
plot(res.aov, 1)
leveneTest(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(2), ] )


### ONE-WAY ANOVA TEST: +info: http://www.sthda.com/english/wiki/one-way-anova-test-in-r
# Null hypothesis: the means of the different groups are the same
# Alternative hypothesis: At least one sample mean is not equal to the others.
# if p-value is small, it means there is at least one group with a mean value with a significant difference

# according to the type of test for level 0
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0), ] )
summary(res.aov)

## according to the type (without BASE) of test for level 0
## I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
# res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(0) & in_data_ball$type_test==c(' Haptic', ' Auditory', ' HapticAuditory') , ] )
# summary(res.aov)

# according to the type of test for level 1
# I am expecting that all means are similar, i.e. there is no significant difference, i.e. a big p-value
res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==c(1), ] )
summary(res.aov)

# according to the type of test for level 2
# I am expecting that all means have a significant difference, i.e. a small p-value
res.aov <- aov(reaction_time_ms ~ type_test, data = in_data_ball[ in_data_ball$level==2, ] )
summary(res.aov)

# in level 2 make anova comparison pairwise
# Tukey multiple pairwise-comparisons. + info: http://www.sthda.com/english/wiki/one-way-anova-test-in-r
#
# tukey's assumptions:
  # 1- The observations being tested are independent within and among the groups.
  # 2- The groups associated with each mean in the test are normally distributed.
  # 3- There is equal within-group variance across the groups associated with each mean in the test (homogeneity of variance). (Use the levenes test)
  #     --> to test 3, the Levene test: 
  #                   - the null hypothesis is that all populations variances are equal;
  #                   - the alternative hypothesis is that at least two of them differ.
  #         For levene test, I expect that I cannot reject the null H, i.e. a big p-value
leveneTest( reaction_time_ms ~ type_test , data = in_data_ball[ in_data_ball$level==2, ])
  
# diff: difference between means of the two groups
# lwr, upr: the lower and the upper end point of the confidence interval at 95% (default)
# p adj: p-value after adjustment for the multiple comparisons.
  
TukeyHSD(res.aov)

# In some part I read TukeyTest is better than t-test. Thus, I am NOT GONNA do t-test for comparison
# t-test to see if a group has a greater mean value than the other


#################
# non-parametric Kruskal-wallis test
kruskal.test(reaction_time_ms ~ type_test , data = in_data_ball[ in_data_ball$level==0, ])
kruskal.test(reaction_time_ms ~ type_test , data = in_data_ball[ in_data_ball$level==1, ])
kruskal.test(reaction_time_ms ~ type_test , data = in_data_ball[ in_data_ball$level==2, ])
#################


#
#######################################################################################################################
################ <Analyzing reaction_time_ms based on initial system position>  #######################################
#######################################################################################################################
## users can have a first react depending on the number they have to move the ball. Faster for further distances and slow for closer distances
##

library(dplyr)
library(ggpubr)
library(car)

### read data
input_path = './Downloads/Proyecto - Driving stress/Data/Data_cogito/Processed_data/final_tests/2_filter2/'
input_file_ball = 'ball_data.csv'

in_data_ball <- read.csv( paste(input_path,input_file_ball, sep ='') , sep = ';' )
# Transform none to na
in_data_ball <- na_if(in_data_ball, 'None')
# transform to numeric values
in_data_ball[, 6]  <- as.logical(in_data_ball[, 6])
in_data_ball[, c(1, 3:5, 7:8)]  = as.numeric( unlist( in_data_ball[, c(1, 3:5, 7:8)] ) )
in_data_ball$type_test <- as.factor(in_data_ball$type_test)
str(in_data_ball)

## I DECIDED TO USE THIS OPTION: where success failed, replace values in reaction_time_ms by na values
in_data_ball[ in_data_ball$success == FALSE, ]$reaction_time_ms <- NA
head(in_data_ball)

## removing outliers with small values (see above code for further explanation)
in_data_ball$reaction_time_ms[ in_data_ball$reaction_time_ms < 80 ] <- NA

in_data_ball2 <- in_data_ball

## BOXPLOTS
# Box plots by type of tests. Level 0
ggboxplot(in_data_ball2[ in_data_ball2$level==0, ] , x = "system_ball", y = "reaction_time_ms", 
          #color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          #order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Test type", title = "Response time in LEVEL 0" ,
)

# Box plots by type of tests. Level 1
ggboxplot(in_data_ball2[ in_data_ball2$level==1, ] , x = "system_ball", y = "reaction_time_ms", 
          #color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          #order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Test type", title = "Response time in LEVEL 1" ,
)

# Box plots by type of tests. Level 2
ggboxplot(in_data_ball2[ in_data_ball2$level==2, ] , x = "system_ball", y = "reaction_time_ms", 
          #color = "type_test", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue"),
          #order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Test type", title = "Response time in LEVEL 2" ,
)

####
####
## taking absolute values
in_data_ball2$system_ball <- abs(in_data_ball2$system_ball)
####
####

# Box plots by type of tests. Level 0
ggboxplot(in_data_ball2[ in_data_ball2$level==0, ] , x = "system_ball", y = "reaction_time_ms", 
          color = "system_ball", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue", 'black', 'orange'),
          #order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Absolute of Initial Position", title = "Response time in LEVEL 0" ,
)

# Box plots by type of tests. Level 1
ggboxplot(in_data_ball2[ in_data_ball2$level==1, ] , x = "system_ball", y = "reaction_time_ms", 
          color = "system_ball", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue", 'black', 'orange'),
          #order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Absolute of Initial Position", title = "Response time in LEVEL 1" ,
)

# Box plots by type of tests. Level 2
ggboxplot(in_data_ball2[ in_data_ball2$level==2, ] , x = "system_ball", y = "reaction_time_ms", 
          color = "system_ball", palette = c("#00AFBB", "#E7B800", "#FC4E07", "blue", 'black', 'orange'),
          #order = c(" Base", " Auditory", " Haptic", " HapticAuditory"),
          ylab = "Response Time", xlab = "Absolute of Initial Position", title = "Response time in LEVEL 2" ,
)


###########
###########
## Analyzing only extreme positions 1 and 6
in_data_ball2 %>% count(system_ball)
in_data_ball2 <- filter(in_data_ball2, system_ball %in% c(1,6))
# in_data_ball2[ in_data_ball2$system_ball == c(1,6), ] %>% count(system_ball) # this command fails, I do not why
in_data_ball2 %>% count(system_ball)
###########
###########


in_data_ball2_BACKUP <- in_data_ball2

level = 0
position = 1
hist( in_data_ball2[ (in_data_ball2$level==level & in_data_ball2$system_ball==position), "reaction_time_ms"] , 20  )
ggqqplot( (in_data_ball2[ (in_data_ball2$level==level & in_data_ball2$system_ball==position), "reaction_time_ms"] ))


### making data to have a normal distribution
### applying log function as transformation
in_data_ball2$reaction_time_ms <- log(in_data_ball2$reaction_time_ms )
in_data_ball2[ is.infinite(in_data_ball2$reaction_time_ms), ]  <- NA


# defining a function to compute anova with their assumptions and tukey as post-hoc test
anovaAnalysis <- function(level, position, varY, dataBall){
  # testing normality
  hist( dataBall[ (dataBall$level==level & dataBall$system_ball==position), varY ] , 20  )
  plot(ggqqplot( dataBall[ (dataBall$level==level & dataBall$system_ball==position), varY ] ))
  
  # testing homoscedasticity
  res.aov <- aov( eval(as.symbol(varY)) ~ type_test , data = dataBall[ (dataBall$level==level & dataBall$system_ball==position),  ]  )
  plot(res.aov, 1)
  print( leveneTest( eval(as.symbol(varY)) ~ type_test , data = dataBall[ (dataBall$level==level & dataBall$system_ball==position),  ]  ) )
  
  # anova results
  print('Anova: ')
  print(summary(res.aov))
  
  #post-hoc Test
  print("Tukey test")
  TukeyHSD(res.aov)
}

# i will analyse only extreme positions (1 and 6) because for users is is easier to identified
# how far they are from the target position, in comparison to middle positions (2,3,4,5).

# level 0
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 0, position = 1, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 0, position = 2, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 0, position = 3, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 0, position = 4, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 0, position = 5, varY = "reaction_time_ms", dataBall = in_data_ball2)
anovaAnalysis(level = 0, position = 6, varY = "reaction_time_ms", dataBall = in_data_ball2)

# level 1
# expecting. levene with big p-value
# expecting. anova  with big p-value
anovaAnalysis(level = 1, position = 1, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 1, position = 2, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 1, position = 3, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 1, position = 4, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 1, position = 5, varY = "reaction_time_ms", dataBall = in_data_ball2)
anovaAnalysis(level = 1, position = 6, varY = "reaction_time_ms", dataBall = in_data_ball2)

# level 2
# expecting. levene with big p-value
# expecting. anova  with small p-value
anovaAnalysis(level = 2, position = 1, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 2, position = 2, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 2, position = 3, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 2, position = 4, varY = "reaction_time_ms", dataBall = in_data_ball2)
# anovaAnalysis(level = 2, position = 5, varY = "reaction_time_ms", dataBall = in_data_ball2)
anovaAnalysis(level = 2, position = 6, varY = "reaction_time_ms", dataBall = in_data_ball2)



##########
##########


